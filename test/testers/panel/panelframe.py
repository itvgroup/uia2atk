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
    #PANEL_ONE = "panel1"
    #PANEL_TWO = "panel2"
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

    def __init__(self, accessible):
        super(PanelFrame, self).__init__(accessible)
        #self.panel1 = self.findPanel(self.PANEL_ONE)
        #self.panel2 = self.findPanel(self.PANEL_TWO)
        self.panel = self.findAllPanels(None)
        self.panel1 = self.panel[0]
        self.panel2 = self.panel[1]

        self.check1 = self.findCheckBox(self.CHECK_ONE)
        self.check2 = self.findCheckBox(self.CHECK_TWO)
        self.check3 = self.findCheckBox(self.CHECK_THREE)
        self.check4 = self.findCheckBox(self.CHECK_FOUR)
        self.radio1 = self.findRadioButton(self.RADIO_ONE)
        self.radio2 = self.findRadioButton(self.RADIO_TWO)
        self.radio3 = self.findRadioButton(self.RADIO_THREE)
        self.label1 = self.findLabel(self.LABEL_ONE)
        self.label2 = self.findLabel(self.LABEL_TWO)
        self.label3 = self.findLabel(self.LABEL_THREE)

    # give 'click' action
    def click(self, button):
        button.click()

    # check the Label's text after click button
    def assertLabel(self, labelText):
        procedurelogger.expectedResult('Label text has been changed to "%s"' 
                                        % labelText)
        self.findLabel(labelText)
    
    # close application main window after running test
    def quit(self):
        self.altF4()