/**
 * An enumeration of states that the save button can be in.
 */
enum SaveButtonMode {
    /**
     * The save button should give feedback that the save process failed.
     */
    FAILURE,

    /**
     * The save button should draw as normal.
     */
    NORMAL,

    /**
     * The save button should give feedback that the save process is in progress.
     */
    SAVING,

    /**
     * The save button should give feedback that the save process completed.
     */
    SAVED,

    _UNUSED
}

export default SaveButtonMode;