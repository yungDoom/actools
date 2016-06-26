﻿using System;
using System.Diagnostics;
using System.IO;
using AcManager.Tools.AcErrors;
using AcManager.Tools.AcManagersNew;
using AcManager.Tools.Helpers;
using FirstFloor.ModernUI.Windows.Controls;
using JetBrains.Annotations;

namespace AcManager.Tools.AcObjectsNew {
    public abstract partial class AcCommonObject : AcObjectNew {
        public const string AuthorKunos = "Kunos";

        public readonly IFileAcManager FileAcManager;

        public virtual bool NeedsMargin => false;

        // ReSharper disable once NotNullMemberIsNotInitialized
        protected AcCommonObject(IFileAcManager manager, string id, bool enabled)
                : base(manager, id, enabled) {
            FileAcManager = manager;
        }

        [NotNull]
        public string Location { get; protected set; }

        protected virtual string GetLocation() {
            return FileAcManager.Directories.GetLocation(Id, Enabled);
        }

        private bool _locationsInitialized;

        protected void InitializeLocationsOnce() {
            if (_locationsInitialized) return;
            InitializeLocations();
            _locationsInitialized = true;
        }

        protected virtual void InitializeLocations() {
            Location = GetLocation();
        }

        private bool _isNew;

        public bool IsNew {
            get { return _isNew; }
            set {
                if (Equals(value, _isNew)) return;
                _isNew = value;
                OnPropertyChanged();
            }
        }

        public override void Reload() {
            ClearErrors();
            LoadOrThrow();
            Changed = false;
        }

        private DateTime _creationTime;

        public void CheckIfNew() {
            try {
                IsNew = DateTime.Now - _creationTime < SettingsHolder.Content.NewContentPeriod.TimeSpan;
            } catch (Exception) {
                IsNew = false;
            }
        }

        protected bool LoadingInProcess { get; private set; }

        protected abstract void LoadOrThrow();

        public sealed override void Load() {
            InitializeLocationsOnce();

            ClearErrors();
            Changed = false;
            LoadingInProcess = true;

            _creationTime = File.GetCreationTime(Location);
            CheckIfNew();

            try {
                LoadOrThrow();
            } catch (AcErrorException e) {
                AddError(e.AcError);
#if !DEBUG
            } catch (Exception e) {
                AddError(AcErrorType.Load_Base, e);
#endif
            } finally {
                LoadingInProcess = false;
            }
        }

        public void SortAffectingValueChanged() {
            if (!LoadingInProcess) {
                Manager.UpdateList();
            }
        }

        public abstract bool HasData { get; }
        
        public override string Name {
            get { return base.Name; }
            protected set {
                value = value?.Trim();

                if (Equals(value, base.Name)) return;
                base.Name = value;

                ErrorIf(string.IsNullOrEmpty(value) && HasData, AcErrorType.Data_ObjectNameIsMissing);
                SortAffectingValueChanged();

                OnPropertyChanged(nameof(NameEditable));
                Changed = true;
            }
        }

        [CanBeNull]
        public virtual string NameEditable {
            get { return Name ?? Id; }
            set { Name = value; }
        }

        private bool _changed;

        public virtual bool Changed {
            get { return _changed; }
            protected set {
                if (value == _changed || LoadingInProcess) return;
                _changed = value;
                OnPropertyChanged(nameof(Changed));
            }
        }

        private int? _year;

        public int? Year {
            get { return _year; }
            set {
                if (value == 0) {
                    value = null;
                }

                if (value == _year) return;
                _year = value;
                OnPropertyChanged(nameof(Year));

                if (_year.HasValue && Name != null) {
                    var inName = AcStringValues.GetYearFromName(Name);
                    if (inName.HasValue && inName.Value != _year.Value) {
                        Name = AcStringValues.NameReplaceYear(Name, _year.Value);
                    }
                }

                Changed = true;
            }
        }

        public abstract void Save();

        public virtual void ViewInExplorer() {
            if (File.GetAttributes(Location).HasFlag(FileAttributes.Directory)) {
                WindowsHelper.ViewDirectory(Location);
            } else {
                WindowsHelper.ViewFile(Location);
            }
        }

        protected virtual void Toggle() {
            FileAcManager.Toggle(Id);
        }

        protected virtual void Rename(string newId) {
            FileAcManager.Rename(Id, newId, Enabled);
        }

        public virtual void Delete() {
            FileAcManager.Delete(Id);
        }

        public virtual bool HandleChangedFile(string filename) => false;

        [Obsolete]
        protected void OnImageChanged(string propertyName) {
            BetterImage.ReloadImage((string)GetType().GetProperty(propertyName).GetValue(this, null));
        }

        protected void OnImageChangedValue(string filename) {
            BetterImage.ReloadImage(filename);
        }
    }
}
