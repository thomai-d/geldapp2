using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace GeldApp2.Extensions
{
    public static class DisposableExtensions
    {
        public static void DisposeWith(this IDisposable disp, CompositeDisposable disposables)
        {
            disposables.Add(disp);
        }
    }
}
