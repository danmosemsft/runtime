using System;
using System.Collections.ObjectModel;
using System.Speech.Internal;

namespace System.Speech.Recognition.SrgsGrammar
{
    /// <summary>
    /// Summary description for SrgsItemList.
    /// </summary>
    [Serializable]
    internal class SrgsItemList : Collection<SrgsItem>
    {
        //*******************************************************************
        //
        // Interfaces Implementations
        //
        //*******************************************************************

        #region Interfaces Implementations

        protected override void InsertItem (int index, SrgsItem item)
        {
            Helpers.ThrowIfNull (item, "item");

            base.InsertItem (index, item);
        }

        #endregion
    }
}
