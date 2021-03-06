# vim: set tabstop=4 shiftwidth=4 expandtab
##############################################################################
# Written by:  Cachen Chen <cachen@novell.com>
# Date:        10/21/2008
# Description: panel.py wrapper script
#              be called by ../panel_basic_ops.py
##############################################################################$

"""Application wrapper for panel.py"""

from strongwind import *

class PanelFrame(accessibles.Frame):

    # constants
    # the available widgets on the window
    CHECK_ONE = "Bananas"
    CHECK_TWO = "Chicken"
    CHECK_THREE = "Stuffed Peppers"
    CHECK_FOUR = "Beef"
    RADIO_ONE = "Male"
    RADIO_TWO = "Female"
    RADIO_THREE = "Disabled"
    LABEL_ONE = "multi-choose:"
    LABEL_TWO = "Tell Me Your Gender:"
    LABEL_THREE = "Go On:____"

    PANELS_NUM = 2

    def __init__(self, accessible):
        super(PanelFrame, self).__init__(accessible)
        self.panels = self.findAllPanels(None)

        assert len(self.panels) == self.PANELS_NUM, \
                       "actual number of panels:%s, expected:%s" % \
                           (len(self.panels), self.PANELS_NUM)

        self.panel1 = self.panels[0]
        self.panel2 = self.panels[1]

        # find all children in panel1
        self.check1 = self.panel1.findCheckBox(self.CHECK_ONE)
        self.check2 = self.panel1.findCheckBox(self.CHECK_TWO)
        self.check3 = self.panel1.findCheckBox(self.CHECK_THREE)
        self.check4 = self.panel1.findCheckBox(self.CHECK_FOUR)
        self.label1 = self.panel1.findLabel(self.LABEL_ONE)

        # find all children in panel2
        self.radio1 = self.panel2.findRadioButton(self.RADIO_ONE)
        self.radio2 = self.panel2.findRadioButton(self.RADIO_TWO)
        self.radio3 = self.panel2.findRadioButton(self.RADIO_THREE)
        self.label2 = self.panel2.findLabel(self.LABEL_TWO)
        self.label3 = self.panel2.findLabel(self.LABEL_THREE)

    def assertText(self, accessible, expected_text):
        """make sure accessible's text is expected"""
        procedurelogger.action("check %s's text" % accessible)
        procedurelogger.expectedResult('%s\'s text is "%s"' % \
                                            (accessible, expected_text))
        assert accessible.text == expected_text, \
                               'actual text is "%s", expected text is "%s"' % \
                               (accessible.text, expected_text)

    # close application main window after running test
    def quit(self):
        self.altF4()
