﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ASCompletion.Context;
using ASCompletion;
using ASCompletion.Model;
using System.Collections;
using System.IO;

namespace NavigationBar.Managers
{
    public class NavigationManager
    {
        private static NavigationManager _instance = null;

        public static NavigationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NavigationManager();
                }

                return _instance;
            }
        }

        private FixedSizeStack<NavigationLocation> _backwardStack = null;
        private FixedSizeStack<NavigationLocation> _forwardStack = null;
        private NavigationLocation _currentLocation = null;
        private Timer _updateTimer = null;

        public event EventHandler LocationChanged;

        public NavigationManager()
        {
            _backwardStack = new FixedSizeStack<NavigationLocation>(100);
            _forwardStack = new FixedSizeStack<NavigationLocation>(100);

            _updateTimer = new Timer();
            _updateTimer.Interval = 200;
            _updateTimer.Tick += new EventHandler(timer_Tick);
            _updateTimer.Start();
        }

        public NavigationLocation CurrentLocation
        {
            get
            {
                return _currentLocation;
            }
        }

        public bool CanNavigateForward
        {
            get
            {
                return _forwardStack.Count > 0;
            }
        }

        public bool CanNavigateBackward
        {
            get
            {
                return _backwardStack.Count > 0;
            }
        }

        public void NavigateForward()
        {
            if (!CanNavigateForward)
                return;

            _backwardStack.Push(_currentLocation);
            _currentLocation = _forwardStack.Pop();
            NavigateTo(_currentLocation);
        }

        public void NavigateBackward()
        {
            if (!CanNavigateBackward)
                return;

            _forwardStack.Push(_currentLocation);
            _currentLocation = _backwardStack.Pop();
            NavigateTo(_currentLocation);
        }

        internal void NavigateBackwardTo(NavigationLocation location)
        {
            while (_currentLocation != location)
            {
                _forwardStack.Push(_currentLocation);
                _currentLocation = _backwardStack.Pop();
            }
            NavigateTo(_currentLocation);
        }

        internal IEnumerable<NavigationLocation> BackwardHistory
        {
            get { return _backwardStack; }
        }

        internal void NavigateForwardTo(NavigationLocation location)
        {
            while (_currentLocation != location)
            {
                _backwardStack.Push(_currentLocation);
                _currentLocation = _forwardStack.Pop();
            }
            NavigateTo(_currentLocation);
        }

        internal IEnumerable<NavigationLocation> ForwardHistory
        {
            get { return _forwardStack; }
        }

        public void Clear()
        {
            _forwardStack.Clear();
            _backwardStack.Clear();
            OnLocationChanged();
        }

        private void NavigateTo(NavigationLocation location)
        {
            ModelsExplorer.Instance.OpenFile(location.FilePath);

            if (ASContext.CurSciControl != null &&
                ASContext.Context != null &&
                ASContext.Context.CurrentFile == location.FilePath)
            {
                ASContext.CurSciControl.GotoPos(location.Position);
            }

            OnLocationChanged();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            _updateTimer.Stop();

            if (HasLocationChanged)
                if (_currentLocation != null)
                    AddCurrentLocation();
                else
                    _currentLocation = GetCurrentLocation();

            _updateTimer.Start();
        }

        private bool HasLocationChanged
        {
            get
            {
                // If we do not have a previous location then we've moved
                if (_currentLocation == null)
                    return true;

                // If there is no text editor then don't track
                if (ASContext.CurSciControl == null)
                    return false;

                // If there is no context then don't track
                if (ASContext.HasContext == false)
                    return false;

                // If we have changed files then we've moved
                if (_currentLocation.FilePath != ASContext.Context.CurrentFile)
                    return true;

                if (_currentLocation.Position == ASContext.CurSciControl.CurrentPos)
                    return false;

                // If the current class has changed then we've moved
                string currentClass = ASContext.Context.CurrentClass == null ? string.Empty : ASContext.Context.CurrentClass.Name;
                if (_currentLocation.ClassName != currentClass)
                    return true;

                // If the current member has changed then we've moved
                string currentMember = ASContext.Context.CurrentMember == null ? string.Empty : ASContext.Context.CurrentMember.Name;
                if (_currentLocation.MemberName != currentMember)
                    return true;

                // If the current member flags have changed then we've moved
                if (!string.IsNullOrEmpty(currentMember) &&
                    _currentLocation.MemberFlags != ASContext.Context.CurrentMember.Flags)
                    return true;

                return false;
            }
        }

        private void AddCurrentLocation()
        {
            if (CanNavigateForward)
                _forwardStack.Clear();

            var lastLocation = _currentLocation;
            _currentLocation = GetCurrentLocation();

            // Is a new member name being typed?
            if ((lastLocation.FilePath == _currentLocation.FilePath &&
                lastLocation.LineFrom == _currentLocation.LineFrom) ||
                lastLocation.MemberName == _currentLocation.MemberName)
                return;

            if (!string.IsNullOrEmpty(_currentLocation.MemberName))
                _backwardStack.Push(lastLocation);

            OnLocationChanged();
        }

        private NavigationLocation GetCurrentLocation()
        {
            NavigationLocation location = new NavigationLocation();

            if (ASContext.CurSciControl == null || ASContext.HasContext == false)
                return null;

            string currentClass = ASContext.Context.CurrentClass == null ? string.Empty : ASContext.Context.CurrentClass.Name;
            string currentMember = ASContext.Context.CurrentMember == null ? string.Empty : ASContext.Context.CurrentMember.Name;

            location.FilePath = ASContext.Context.CurrentFile;
            location.ClassName = currentClass;
            location.MemberName = currentMember;
            location.MemberFlags = string.IsNullOrEmpty(currentMember) ? FlagType.Class : ASContext.Context.CurrentMember.Flags;
            location.LineFrom = ASContext.Context.CurrentLine;
            location.Position = ASContext.CurSciControl.CurrentPos;

            return location;
        }

        private void OnLocationChanged()
        {
            if (LocationChanged != null)
                LocationChanged(this, new EventArgs());
        }
    }

    #region Custom Structures

    public class NavigationLocation
    {
        public string FilePath { get; set; }
        public int Position { get; set; }
        public string ClassName { get; set; }
        public string MemberName { get; set; }
        public FlagType MemberFlags { get; set; }
        public int LineFrom { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(MemberName))
                return string.Format("{0} {1} Line: {2}", Path.GetFileName(FilePath), ClassName, LineFrom);
            else
                return string.Format("{0} {1}.{2} Line: {3}", Path.GetFileName(FilePath), ClassName, MemberName, LineFrom);
        }
    }

    class FixedSizeStack<T> : IEnumerable<T>
    {
        private List<T> _list = new List<T>();
        private int _maxSize = 10;

        public FixedSizeStack(int size)
        {
            if (size > 0)
                _maxSize = size;
        }

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public T Pop()
        {
            T item = _list[0];
            _list.RemoveAt(0);
            return item;
        }

        public void Push(T item)
        {
            _list.Insert(0, item);

            if (_list.Count >= _maxSize)
                _list.RemoveAt(_maxSize - 1);
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }

    #endregion
}
