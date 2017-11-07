using MyHerbalife3.Shared.UI.Helpers;
using System;
using System.ComponentModel;
using System.Web.UI;

namespace HL.MyHerbalife.Web.Controls.Content
{
    /// <summary>
    ///     Provides UI feedback of asynchronous postbacks with the UpdatePanel.
    ///     Control will also help prevent multiple postbacks by overlaying the target
    ///     UpdatePanel with a div.
    /// </summary>
    public partial class UpdatePanelProgressIndicator : UserControl
    {
        /// <summary>
        ///     Target control for the progress indicator.
        ///     This should be an AJAX Toolkit UpdatePanel.
        /// </summary>
        [Browsable(true)]
        public string TargetControlID { get; set; }

        /// <summary>
        ///     Client ID of the target control.
        /// </summary>
        [Browsable(false)]
        public string TargetControlClientID { get; private set; }

        /// <summary>
        ///     Reference to the target control.
        /// </summary>
        private UpdatePanel _targetControl;

        /// <summary>
        ///     Client side script which will be fired when updating and updated.
        /// </summary>
        private static string _clientScript =
            @"function aeProgress_onUpdating(progressDivID, targetControlID) {
    var updateProgressDiv = $get(progressDivID);
    updateProgressDiv.style.display = '';
    var targetControl = $get(targetControlID);
    var targetControlBounds = Sys.UI.DomElement.getBounds(targetControl);
    Sys.UI.DomElement.setLocation(updateProgressDiv, targetControlBounds.x, targetControlBounds.y);
    updateProgressDiv.style.width = targetControlBounds.width + 'px';
    updateProgressDiv.style.height = targetControlBounds.height + 'px';
}
var globalProgressDivID = '';
function aeProgress_onUpdated(progressDivID) {
    globalProgressDivID = progressDivID;
    var updateProgressDiv = $get(progressDivID);
    if(updateProgressDiv)
    {
        updateProgressDiv.style.display = 'none';
    }
}
var currentPostBackElement;
function pageLoad() {
    var manager = Sys.WebForms.PageRequestManager.getInstance();
    manager.add_initializeRequest(OnInitializeRequest);
}
function OnInitializeRequest(sender, args) {
    var manager = Sys.WebForms.PageRequestManager.getInstance();
    currentPostBackElement = args.get_postBackElement();
}
function checkParentControlExists(childControl, parentControlID) {
    var currentControl = $get(childControl.id);
    do {
        if(currentControl.id == parentControlID) { return true; }
        currentControl = currentControl.parentNode;
    } while (currentControl != null);
    return false;
}";

        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager.RegisterClientScriptBlock(this, GetType(), "UpdatePanelProgressIndicator", _clientScript, true);

            // Target control must exist at this controls level or higher up the hierarchy.
            _targetControl = MyHLWebUtil.FindChildControl<UpdatePanel>(Parent, TargetControlID);

            // Make sure target control could be located.
            if (_targetControl == null)
            {
                throw new ArgumentException("Unable to find an UpdatePanel control with ID: " + TargetControlID);
            }

            aeProgress.TargetControlID = _targetControl.ID;
            TargetControlClientID = _targetControl.ClientID;

            // Hack due to "The 'Animations' property of 'ajaxToolkit:UpdatePanelAnimationExtender' does not allow child objects."
            aeProgress.Animations = aeProgress.Animations.Replace("{progressDivClientID}", updateProgressDiv.ClientID);
            aeProgress.Animations = aeProgress.Animations.Replace("{targetControlClientID}", TargetControlClientID);
            aeProgress.Animations = aeProgress.Animations.Replace("{alwaysUpdate}",
                                                                  (_targetControl.UpdateMode ==
                                                                   UpdatePanelUpdateMode.Always
                                                                       ? "true"
                                                                       : "false"));
        }
    }
}