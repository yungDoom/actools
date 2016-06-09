﻿using System.Threading.Tasks;
using AcManager.Tools.Helpers;
using AcManager.Tools.Objects;
using AcTools.Processes;

namespace AcManager.Pages.Drive {
    public partial class QuickDrive_TimeAttack : IQuickDriveModeControl {
        public class QuickDrive_TimeAttackViewModel : QuickDriveModeViewModel {
            private bool _penalties;

            public bool Penalties {
                get { return _penalties; }
                set {
                    if (Equals(value, _penalties)) return;
                    _penalties = value;
                    OnPropertyChanged();
                    SaveLater();
                }
            }

            private class SaveableData {
                public bool Penalties;
            }

            public QuickDrive_TimeAttackViewModel(bool initialize = true) {
                Saveable = new SaveHelper<SaveableData>("__QuickDrive_TimeAttack", () => new SaveableData {
                    Penalties = Penalties,
                }, o => {
                    Penalties = o.Penalties;
                }, () => {
                    Penalties = true;
                });

                if (initialize) {
                    Saveable.Initialize();
                } else {
                    Saveable.Reset();
                }
            }

            public override async Task Drive(Game.BasicProperties basicProperties, Game.AssistsProperties assistsProperties,
                    Game.ConditionProperties conditionProperties, Game.TrackProperties trackProperties) {
                await StartAsync(new Game.StartProperties {
                    BasicProperties = basicProperties,
                    AssistsProperties = assistsProperties,
                    ConditionProperties = conditionProperties,
                    TrackProperties = trackProperties,
                    ModeProperties = new Game.TimeAttackProperties {
                        Penalties = Penalties
                    }
                });
            }
        }

        public QuickDrive_TimeAttack() {
            InitializeComponent();
            // DataContext = new QuickDrive_TimeAttackViewModel();
        }


        public QuickDriveModeViewModel Model {
            get { return (QuickDriveModeViewModel)DataContext; }
            set { DataContext = value; }
        }
    }
}
