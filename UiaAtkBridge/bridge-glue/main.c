// Permission is hereby granted, free of charge, to any person obtaining 
// a copy of this software and associated documentation files (the 
// "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to 
// the following conditions: 
//  
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software. 
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// 
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com) 
// 
// Authors: 
//      Andres G. Aragoneses <aaragoneses@novell.com>
//      Mike Gorse <mgorse@novell.com>
// 

#include "main.h"

/* Add function definitions here */

//this glue library is born from the need of a workaround to bug#386950, bringing optimization, and is
// based on r101690 ( http://anonsvn.mono-project.com/source/trunk/uia2atk/src/patches/bug365437comm2.diff )
// TODO: rewrite in Vala?? :)

static GHashTable *listener_list = NULL;

guint _add_global_event_listener(
	GSignalEmissionHook listener,
	const gchar *event_type);

void _remove_global_event_listener (guint remove_listener);

void
override_global_event_listener ()
{
	AtkUtilClass *klass = g_type_class_peek (ATK_TYPE_UTIL);
	if (!klass)
		klass = g_type_class_ref (ATK_TYPE_UTIL);
	((AtkUtilClass *) klass)->add_global_event_listener = _add_global_event_listener;
	((AtkUtilClass *) klass)->remove_global_event_listener = _remove_global_event_listener;
}

static void
_listener_info_destroy (gpointer data)
{
	g_free(data);
}

static gint listener_idx = 1;

typedef struct _AtkUtilListenerInfo AtkUtilListenerInfo;

struct _AtkUtilListenerInfo
{
	gint key;
	guint signal_id;
	gulong hook_id;
};


guint
_atksharp_add_listener (GSignalEmissionHook listener,
                        const gchar *object_type,
                        const gchar *signal,
                        const gchar *hook_data)
{
	GType type;
	guint signal_id;
	gint rc = 0;

	if (!strncmp ("window", object_type, 7))
		type = ATK_TYPE_OBJECT;
	else
		type = g_type_from_name (object_type);
	
	if (type)
	{
		signal_id = g_signal_lookup (signal, type);

		if (signal_id > 0) {

			AtkUtilListenerInfo *listener_info;

			rc = listener_idx;

			listener_info = g_malloc(sizeof(AtkUtilListenerInfo));
			listener_info->key = listener_idx;
			listener_info->hook_id =
			
			g_signal_add_emission_hook (signal_id, 0, listener,
			                            g_strdup (hook_data),
			                            (GDestroyNotify) g_free);
			listener_info->signal_id = signal_id;

			g_hash_table_insert(listener_list, &(listener_info->key), listener_info);
			listener_idx++;
		} else {
			g_warning("Invalid signal type %s\n", signal);
		}
	} else {
		g_warning("Invalid object type %s\n", object_type);
	}
	
	return rc;
}

guint _add_global_event_listener (
	GSignalEmissionHook listener,
	const gchar *event_type)
{
	if (listener_list == NULL)
		listener_list = g_hash_table_new_full(g_int_hash, g_int_equal, NULL, _listener_info_destroy);

	guint rc = 0;
	gchar **split_string;

	//split_string[0]: toolkit
	//            [1]: class/interface
	//            [2]: event type
	// example: Gtk:AtkObject:children-changed
	split_string = g_strsplit (event_type, ":", 3);

	if (split_string)
	{
		if (!strncmp ("window", split_string[0], 7))
			rc = _atksharp_add_listener (listener, "window", split_string[1], event_type);
		else
			rc = _atksharp_add_listener (listener, split_string[1], split_string[2], event_type);

		g_strfreev (split_string);
	}

	return rc;
}

void
_remove_global_event_listener (guint remove_listener)
{
  if (remove_listener > 0)
  {
    AtkUtilListenerInfo *listener_info;
    gint tmp_idx = remove_listener;

    listener_info = (AtkUtilListenerInfo *)
      g_hash_table_lookup(listener_list, &tmp_idx);

    if (listener_info != NULL)
      {
        /* Hook id of 0 and signal id of 0 are invalid */
        if (listener_info->hook_id != 0 && listener_info->signal_id != 0)
          {
            /* Remove the emission hook */
            g_signal_remove_emission_hook(listener_info->signal_id,
              listener_info->hook_id);

            /* Remove the element from the hash */
            g_hash_table_remove(listener_list, &tmp_idx);
          }
        else
          {
            g_warning("Invalid listener hook_id %ld or signal_id %d\n",
              listener_info->hook_id, listener_info->signal_id);
          }
      }
    else
      {
        g_warning("No listener with the specified listener id %d",
          remove_listener);
      }
  }
  else
  {
    g_warning("Invalid listener_id %d", remove_listener);
  }
}

typedef void (*void_func)();

static gchar *
find_atk_bridge (const gchar *path)
{
  gchar *full_path = g_strconcat (path, "/modules/libatk-bridge.so", NULL);
  struct stat st;

  if (stat (full_path, &st) == 0)
    return full_path;
  return NULL;
}

static char *
get_path_from_var (const char *var)
{
  const gchar *value = g_getenv (var);
  gchar **values;
  gchar **ptr;
  gchar *path = NULL;

  if (!value)
    return NULL;

  values = g_strsplit (value, ":", 0);
  for (ptr = values; *ptr; ptr++)
    {
      if (!path)
        path = find_atk_bridge (*ptr);
      g_free (*ptr);
    }
  g_free (values);
  return path;
}

static const char *
get_bridge_path ()
{
  static const char *path = NULL;

  if (path)
    return path;

  /* Hack for OpenSUSE: Check GTK_PATH64 first */
  path = get_path_from_var ("GTK_PATH64");
  if (!path)
    path = get_path_from_var ("GTK_PATH");
  if (!path)
  path = g_strconcat (GTK_MODULES_DIR, "/libatk-bridge.so", NULL);
  return path;
}

static void *
get_library ()
{
  static void *library = NULL;
  const char *path;

  if (library)
    return library;
  path = get_bridge_path ();
  library = dlopen (path, RTLD_LAZY);
  if (!library)
    g_warning ("libbridgeglue: Couldn't find atk-bridge: expecting %s\n", path);
  return library;
}

void
gnome_accessibility_module_init ()
{
  void *library = get_library ();
  void_func func;

  if (!library)
    return;
  func = dlsym (library, "gnome_accessibility_module_init");
  if (func)
    func ();
  else
    g_warning ("libbridgeglue: Couldn't find gnome_accessibility_module_init");
}

void
gnome_accessibility_module_shutdown ()
{
  void *library = get_library ();
  void_func func;

  if (!library)
    return;
  func = dlsym (library, "gnome_accessibility_module_shutdown");
  if (func)
    func ();
  else
    g_warning ("libbridgeglue: Couldn't find gnome_accessibility_module_shutdown");
}
