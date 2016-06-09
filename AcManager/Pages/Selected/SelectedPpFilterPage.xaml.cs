﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AcManager.Controls.Helpers;
using AcManager.Controls.Pages.Dialogs;
using AcManager.Tools.Managers;
using AcManager.Tools.Miscellaneous;
using AcManager.Tools.Objects;
using AcManager.Tools.TextEditing;
using AcTools.Utils;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows;
using JetBrains.Annotations;

namespace AcManager.Pages.Selected {
    public partial class SelectedPpFilterPage : ILoadableContent, IParametrizedUriContent {
        public class SelectedPpFilterPageViewModel : SelectedAcObjectViewModel<PpFilterObject> {
            public SelectedPpFilterPageViewModel([NotNull] PpFilterObject acObject) : base(acObject) { }

            public PpFiltersManager Manager => PpFiltersManager.Instance;

            private AsyncCommand _shareCommand;

            public AsyncCommand ShareCommand => _shareCommand ?? (_shareCommand = new AsyncCommand(o => {
                var data = SelectedObject.Content ?? FileUtils.ReadAllText(SelectedObject.Location);
                return SharingUiHelper.ShareAsync(SharedEntryType.PpFilter, SelectedObject.Name, null, data);
            }));

            private RelayCommand _testCommand;

            public RelayCommand TestCommand => _testCommand ?? (_testCommand = new RelayCommand(o => {
                var car = CarsManager.Instance.GetDefault();
                CarOpenInShowroomDialog.Run(car, car?.SelectedSkin?.Id, SelectedObject.AcId);
            }));
        }

        private string _id;

        void IParametrizedUriContent.OnUri(Uri uri) {
            _id = uri.GetQueryParam("Id");
            if (_id == null) {
                throw new Exception("ID is missing");
            }
        }

        private PpFilterObject _object;

        async Task ILoadableContent.LoadAsync(CancellationToken cancellationToken) {
            _object = await PpFiltersManager.Instance.GetByIdAsync(_id);
            _object?.PrepareForEditing();
        }

        void ILoadableContent.Load() {
            _object = PpFiltersManager.Instance.GetById(_id);
            _object?.PrepareForEditing();
        }

        private SelectedPpFilterPageViewModel _model;

        void ILoadableContent.Initialize() {
            if (_object == null) throw new ArgumentException("Can’t find object with provided ID");

            InitializeAcObjectPage(_model = new SelectedPpFilterPageViewModel(_object));
            InputBindings.AddRange(new[] {
                new InputBinding(_model.TestCommand, new KeyGesture(Key.G, ModifierKeys.Control)),
                new InputBinding(_model.ShareCommand, new KeyGesture(Key.PageUp, ModifierKeys.Control)),
            });
            InitializeComponent();

            TextEditor.SetAsIniEditor(v => { _object.Content = v; });
            TextEditor.SetDocument(_object.Content);
            _object.PropertyChanged += SelectedObject_PropertyChanged;
        }

        private void SelectedObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (TextEditor.IsBusy()) return;
            if (e.PropertyName == nameof(_object.Content)) {
                TextEditor.SetDocument(_object.Content);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            _object.PropertyChanged -= SelectedObject_PropertyChanged;
        }
    }
}
