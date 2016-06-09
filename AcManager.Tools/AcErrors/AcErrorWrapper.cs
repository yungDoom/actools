﻿using System.Windows.Input;
using AcManager.Tools.AcObjectsNew;

namespace AcManager.Tools.AcErrors {
    /// <summary>
    /// Why do I need this for? ಠ_ಠ
    /// </summary>
    public abstract class AcErrorWrapper : IAcError {
        protected AcErrorWrapper(IAcError baseError) {
            Target = baseError.Target;
            Category = baseError.Category;
            Type = baseError.Type;
            Message = baseError.Message;
        }

        public IAcObjectNew Target { get; }

        public AcErrorCategory Category { get; }

        public AcErrorType Type { get; }

        public string Message { get; }

        public abstract ICommand StartErrorFixerCommand { get; }
    }
}