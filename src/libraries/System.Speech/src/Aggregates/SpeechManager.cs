//---------------------------------------------------------------------------
//
// <copyright file="SpeechManager.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description:     This class speech-enables an Avalon application from
//                  within by traversing the control tree.
//
// History:
//		2/1/2005	philsch     Created initial version
//---------------------------------------------------------------------------

#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Resources;

using System.Windows.Navigation;
using System.Windows.Input;

using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Speech.Internal;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
#endregion

namespace System.Speech
{
    /// TODOC <_include file='doc\SpeechManager.uex' path='docs/doc[@for="SpeechManager"]/*' />
    
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]
    [Obsolete ("This class will be removed in Beta2 as the same functionalities will be then provided by Hoolie.")]
    public class SpeechManager
    {
        //*******************************************************************
        //
        // Constructors / Destructors
        //
        //*******************************************************************

        #region Constructors / Destructors
        /// TODOC <_include file='doc\SpeechManager.uex' path='docs/doc[@for="SpeechManager.SpeechManager"]/*' />
        public SpeechManager(Application application)
        {
            _speakableElements = new Dictionary<int, SpeechObject>();
            InitializeSpeech(true);

            if (_recognizer != null)
            {
                _application = application;
                application.LoadCompleted += new LoadCompletedEventHandler(OnLoadCompleted);
            }
        }

        /// Empty constructor used by the DRTs
        /// TODOC <_include file='doc\SpeechManager.uex' path='docs/doc[@for="SpeechManager.SpeechManager2"]/*' />
        internal SpeechManager()
        {
            _speakableElements = new Dictionary<int, SpeechObject>();
            InitializeSpeech(false);
        }

        #endregion

        //*******************************************************************
        //
        // Private methods
        //
        //*******************************************************************

        #region Private methods

        #region InitializeSpeech
        // creates the recognizer
        // loads the grammar skeleton from the resources
        // gets a handle to the oneOf lists contained in the grammar skeleton
        private void InitializeSpeech(bool useSpeech)
        {
            ResourceManager resourceManager = new ResourceManager("GrammarFiles", System.Reflection.Assembly.GetExecutingAssembly()); //typeof(SpeechManager).Assembly);
            Stream stream = new MemoryStream((byte[])resourceManager.GetObject(@"UICmds"));

            // load static grammar into our dynamic grammar
            _dynGrammar = new SrgsDocument(new XmlTextReader(stream));

            _invokeList = (SrgsOneOf)_dynGrammar.Rules["InvokeItems"].Elements[0];
            _selectList = (SrgsOneOf)_dynGrammar.Rules["SelectItems"].Elements[0];
            _toggleOnList = (SrgsOneOf)_dynGrammar.Rules["ToggleOnItems"].Elements[0];
            _toggleOffList = (SrgsOneOf)_dynGrammar.Rules["ToggleOffItems"].Elements[0];
            _expandList = (SrgsOneOf)_dynGrammar.Rules["ExpandItems"].Elements[0];
            _focusList = (SrgsOneOf)_dynGrammar.Rules["FocusItems"].Elements[0];

            if (useSpeech)
            {
                try
                {
                    _recognizer = new SpeechRecognizer();
                }
#if DEBUG
                catch (RecognitionNotSupportedException e)
                {
                    Debug.WriteLine(e.Message);
                }
                catch (SpeechRecognizerDisabledException e)
                {
                    Debug.WriteLine(e.Message);
                }
#else
                catch (RecognitionNotSupportedException)
                {
                }
                catch (SpeechRecognizerDisabledException)
                {
                }
#endif
                if (_recognizer != null)
                {
                    _recognizer.SpeechRecognized += new EventHandler<RecognitionEventArgs>(OnSpeechRecognized);
                    _recognizer.RecognizerUpdateReached += new EventHandler<UpdateEventArgs>(UpdateSpeechGrammar);
                }
            }
        }
        #endregion

        #region Grammar updating
        private void RequestUpdateSpeechGrammar()
        {
            // clear all the lists since we are going to rescan the UI now
            _speakableElements.Clear();
            _invokeList.Items.Clear();
            _selectList.Items.Clear();
            _toggleOnList.Items.Clear();
            _toggleOffList.Items.Clear();
            _expandList.Items.Clear();
            _focusList.Items.Clear();

            // rootLabel is used to label controls using the element immediately preceeding
            // them in the tabbing / traversal order; apparently that's the mechanism used
            // by WUIA as well.
            string rootLabel = string.Empty;
            TraverseLogicalTree(_windowWithFocus, ref rootLabel);

            // fix up emtpy oneOf lists
            AddEmptyItemsWhereNecessary();

            // update the recognizer now
            _recognizer.RequestRecognizerUpdate(null);
        }

        // reloads the runtime grammar from the SRGS docuement
        private void UpdateSpeechGrammar(object sender, UpdateEventArgs e)
        {
            _mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal,
                    new RequestUpdateSpeechDelegate(UpdateSpeechGrammarOnCorrectThread));

        }

        private void UpdateSpeechGrammarOnCorrectThread()
        {
            if (_rtGrammar != null)
            {
                _recognizer.UnloadGrammar(_rtGrammar);
            }
            _rtGrammar = new Grammar(_dynGrammar);
            _recognizer.LoadGrammar(_rtGrammar);
        }
        #endregion

        #region Traversing the logical control tree
        private void TraverseLogicalTree(DependencyObject element, ref string label)
        {
            bool doTraversal = true;
            Type elementType = element.GetType();
            SpeechObject so = null;

            if (elementType == typeof(Button))
            {
                so = new ButtonSpeechObject(element, _invokeList, ++_objectIdentifier);
                doTraversal = false;
            }
            else if (elementType == typeof(CheckBox))
            {
                so = (((CheckBox)element).IsChecked == true ) ?
                    (SpeechObject)new CheckBoxSpeechObjectOff(element, _toggleOffList, ++_objectIdentifier) :
                    (SpeechObject)new CheckBoxSpeechObjectOn(element, _toggleOnList, ++_objectIdentifier);
                doTraversal = false;
            }
            else if (elementType == typeof(RadioButton))
            {
                so = new RadioButtonSpeechObject(element, _selectList, ++_objectIdentifier);
                doTraversal = false;
            }
            else if (elementType == typeof(ListBoxItem))
            {
                so = new ListBoxItemSpeechObject(element, _selectList, ++_objectIdentifier);
                doTraversal = false;
            }
            else if (elementType == typeof(ComboBoxItem))
            {
                so = new ComboBoxItemSpeechObject(element, _selectList, ++_objectIdentifier);
                doTraversal = false;
            }
            else if (elementType == typeof(ComboBox))
            {
                so = new ComboBoxSpeechObject(element, label, _expandList, ++_objectIdentifier);
                doTraversal = ((ComboBox)element).IsDropDownOpen;
            }
            else if (elementType == typeof(MenuItem))
            {
                MenuItem mi = (MenuItem)element;
                if (mi.HasItems && mi.IsSubmenuOpen)
                {
                    // traverse the menu structure directly
                    AddMenuItems(mi);
                }
                so = new MenuItemSpeechObject(element, _invokeList, ++_objectIdentifier);
                doTraversal = false;
            }
            else if (elementType == typeof(TextBlock))
            {
                // workaround to remove ':' at the end of a 'label'
                label = ((TextBlock)element).Text.Replace(":", string.Empty);
                doTraversal = false;
            }
            else if (elementType == typeof(TextBox))
            {
                so = new TextBoxSpeechObject(element, label, _recognizer, _focusList, ++_objectIdentifier);
                doTraversal = false;
            }

            if (so != null)
            {
                _speakableElements.Add(so._id, so);
            }

            if (doTraversal)
            {
                try
                {
                    foreach (DependencyObject child in LogicalTreeHelper.GetChildren(element))
                    {
                        TraverseLogicalTree(child, ref label);
                    }
                }
#if DEBUG
                catch (InvalidCastException ex) 
                { 
                    Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, SR.Get (SRID.CannotTraverse),
                        elementType.ToString(), ex.Message));
                }
#else
                catch (InvalidCastException) 
                { 
                }
#endif
            }
        }

        private void AddMenuItems(MenuItem menuItem)
        {
            if (menuItem.IsSubmenuOpen)
            {
                foreach (MenuItem item in menuItem.Items)
                {
                    if (item.IsVisible && (item.Mode != MenuItemMode.Separator))
                    {
                        SpeechObject so = new MenuItemSpeechObject((DependencyObject)item, _invokeList, ++_objectIdentifier);
                        _speakableElements.Add(so._id, so);

                        if (item.HasItems)
                        {
                            AddMenuItems(item);
                        }
                    }
                }
            }
        }
        #endregion

        #region Grammar helpers
        private void AddEmptyItemsWhereNecessary()
        {
            if (_invokeList.Children.Length == 0) _invokeList.Add(EmptyItem());
            if (_selectList.Children.Length == 0) _selectList.Add(EmptyItem());
            if (_toggleOnList.Children.Length == 0) _toggleOnList.Add(EmptyItem());
            if (_toggleOffList.Children.Length == 0) _toggleOffList.Add(EmptyItem());
            if (_expandList.Children.Length == 0) _expandList.Add(EmptyItem());
            if (_focusList.Children.Length == 0) _focusList.Add(EmptyItem());
        }

        // this returns an item with a special ruleref to VOID
        // this allows for the rule to compile even if there are 
        // no speak items in the list!
       static  private SrgsItem EmptyItem()
        {
            SrgsItem item = new SrgsItem();
            item.Elements.Add(SrgsRuleRef.Void);
            return item;
        }
        #endregion

        #region Event handlers
        private void OnLoadCompleted(object sender, EventArgs e)
        {
            _mainWindow = _application.MainWindow;
            _windowWithFocus = _mainWindow;

            _mainWindow.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnGotFocus);
            _mainWindow.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(OnLostFocus);

            RequestUpdateSpeechGrammar();
        }

        // activate the grammar now
        void OnGotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Window window = sender as Window;
            if (window != null)
            {
                _windowWithFocus = window;
                e.Handled = true;

                RequestUpdateSpeechGrammar();
            }
            else
            {
                Debug.WriteLine("OnGotFocus: sender is not Window");
            }
            if (_recognizer != null)
            {
                _recognizer.Enabled = true;
            }
        }

        // deactivate the grammar
        void OnLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_recognizer != null)
            {
                _recognizer.Enabled = false;
            }
        }
        void OnSpeechRecognized (object sender, RecognitionEventArgs e)
        {
            if (e.Result.Semantics.ContainsKey("id"))
            {
                try
                {
                    int id = (int)e.Result.Semantics["id"].Value;
                    SpeechObject so = _speakableElements[id];
                    so.DoAction();
                }
#if DEBUG
                catch (NotSupportedException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
#else
                catch (NotSupportedException)
                {
                }
#endif
            }
            else
            {
                // must have come from input scope recognition -> ignore
            }

            // this can be removed once AsyncOperationsManager is fixed!
            _mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal,
                    new RequestUpdateSpeechDelegate(RequestUpdateSpeechGrammar));
        }

        #endregion

        #endregion

        //*******************************************************************
        //
        // Private DRT Helpers
        //
        //*******************************************************************

        #region Private DRT Helpers
        internal int BuildSpeechGrammar (Window window)
        {
            _speakableElements.Clear();
            string label = string.Empty;
            TraverseLogicalTree((DependencyObject)window, ref label);
            return _speakableElements.Count;
        }

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields
        // Speech
        SpeechRecognizer _recognizer;
        Grammar _rtGrammar;

        SrgsDocument _dynGrammar;
        SrgsOneOf _invokeList;
        SrgsOneOf _selectList;
        SrgsOneOf _toggleOnList;
        SrgsOneOf _toggleOffList;
        SrgsOneOf _expandList;
        SrgsOneOf _focusList;

        static int _objectIdentifier;   // used instead of hash code to 

        private Application _application;
        private Window _mainWindow;
        private Window _windowWithFocus;
        private IDictionary<int, SpeechObject> _speakableElements;

        internal delegate void RequestUpdateSpeechDelegate();
        #endregion
    }
}
