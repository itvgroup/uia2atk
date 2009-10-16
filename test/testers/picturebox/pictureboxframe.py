##############################################################################
# Written by:  Cachen Chen <cachen@novell.com>
# Date:        08/14/2008
# Description: picturebox.py wrapper script
#              Used by the picturebox-*.py tests
##############################################################################$

import sys
import os
import actions
import states

from strongwind import *
from picturebox import *

# class to represent the main window.
class PictureBoxFrame(accessibles.Frame):

    # constants
    # the available widgets on the window
    BUTTON_ONE = "openSUSE"
    LABEL = ""

    def __init__(self, accessible):
        super(PictureBoxFrame, self).__init__(accessible)
        self.button1 = self.findPushButton(self.BUTTON_ONE)
        self.label = self.findLabel(None)
        		
    #give 'click' action
    def click(self,button):
        button.click()

    #check the picture after click button
    def assertPicture(self, picture=None):
        picture1 = "desktop-blue_soccer400x500.jpg"
        picture2 = "universe300x400.jpg"
        def resultMatches():
            if picture == 1:
                procedurelogger.expectedResult('picture has been changed to "%s"' % picture1)
                return self.findLabel("You are watching %s/samples/%s" % (uiaqa_path, picture1))
            if picture == 2:
                procedurelogger.expectedResult('picture has been changed to "%s"' % picture2)
                return self.findLabel("You are watching %s/samples/%s" % (uiaqa_path, picture2))
        assert retryUntilTrue(resultMatches), "Expected picture: %s" % picture

    #check icon implementation
    def assertIcon(self):
        procedurelogger.action("search for Icon role")
        self.icon = self.findIcon(None)

        procedurelogger.expectedResult("shows Icon in accerciser")
        assert self.icon

    # assert the size of an image
    def assertImageSize(self, accessible, width=60, height=38):
        procedurelogger.action("assert %s's image size" % accessible)

        if accessible == self.icon:
            size = accessible._accessible.queryImage().getImageSize()
        else:
            size = accessible.imageSize

        procedurelogger.expectedResult('"%s" image size is %s x %s' %
                                                  (accessible, width, height))

        assert width == size[0], "%s (%s), %s (%s)" %\
                                            ("expected width",
                                              width,
                                             "does not match actual width",
                                              size[0])
        assert height == size[1], "%s (%s), %s (%s)" %\
                                            ("expected height",
                                              height,
                                             "does not match actual height",
                                              size[1])
    
    
    #close application main window after running test
    def quit(self):
        self.altF4()