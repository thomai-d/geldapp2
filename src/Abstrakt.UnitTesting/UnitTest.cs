using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using Moq.AutoMock;
using Moq.Language.Flow;

namespace Abstrakt.UnitTesting
{
    public partial class UnitTest<T> : IDisposable
        where T : class
    {
        private readonly List<Mock> knownMocks = new List<Mock>();

        private T target;

        private AutoMocker autoMocker;

        public UnitTest()
        {
            this.autoMocker = new AutoMocker();
        }

        public T Target
        {
            get
            {
                if (this.target == null)
                    this.target = this.autoMocker.CreateInstance<T>();
                return this.target;
            }
        }

        /* Mocking */

        public void Inject<TMock>(TMock instance)
        {
            this.autoMocker.Use(typeof(TMock), instance);
        }

        public void Raise<TMock>(Action<TMock> eventExpression, EventArgs args)
            where TMock : class
        {
            Mock.Get(this.autoMocker.Get<TMock>()).Raise(eventExpression, args);
        }

        public ISetupGetter<TMock, TProperty> SetupGet<TMock, TProperty>(Expression<Func<TMock, TProperty>> exp)
            where TMock : class
        {
            var mock = Mock.Get(this.autoMocker.Get<TMock>());
            if (!this.knownMocks.Contains(mock))
            {
                this.knownMocks.Add(mock);
            }

            return mock.SetupGet(exp);
        }

        public ISetup<TMock> Setup<TMock>(Expression<Action<TMock>> exp)
            where TMock : class
        {
            var mock = Mock.Get(this.autoMocker.Get<TMock>());
            if (!this.knownMocks.Contains(mock))
            {
                this.knownMocks.Add(mock);
            }

            return mock.Setup(exp);
        }

        public ISetup<TMock, TResult> Setup<TMock, TResult>(Expression<Func<TMock, TResult>> exp)
            where TMock : class
        {
            var mock = Mock.Get(this.autoMocker.Get<TMock>());
            if (!this.knownMocks.Contains(mock))
            {
                this.knownMocks.Add(mock);
            }

            return mock.Setup(exp);
        }

        public void ShouldNotHaveCalled<TMock>(Expression<Action<TMock>> exp)
            where TMock : class
        {
            var mock = Mock.Get(this.autoMocker.Get<TMock>());
            if (!this.knownMocks.Contains(mock))
            {
                this.knownMocks.Add(mock);
            }

            mock.Verify(exp, Times.Never());
        }

        public void ShouldHaveCalled<TMock>(Expression<Action<TMock>> exp, int times)
            where TMock : class
        {
            var mock = Mock.Get(this.autoMocker.Get<TMock>());
            if (!this.knownMocks.Contains(mock))
            {
                this.knownMocks.Add(mock);
            }

            mock.Verify(exp, Times.Exactly(times));
        }

        public void ShouldHaveCalledAtLeast<TMock>(Expression<Action<TMock>> exp, int times)
            where TMock : class
        {
            var mock = Mock.Get(this.autoMocker.Get<TMock>());
            if (!this.knownMocks.Contains(mock))
            {
                this.knownMocks.Add(mock);
            }

            mock.Verify(exp, Times.AtLeast(times));
        }

        public void ShouldNotHaveCalled<TMock, TResult>(Expression<Func<TMock, TResult>> exp)
            where TMock : class
        {
            var mock = Mock.Get(this.autoMocker.Get<TMock>());
            if (!this.knownMocks.Contains(mock))
            {
                this.knownMocks.Add(mock);
            }

            mock.Verify(exp, Times.Never());
        }

        public void ShouldHaveCalled<TMock, TResult>(Expression<Func<TMock, TResult>> exp, int times)
            where TMock : class
        {
            var mock = Mock.Get(this.autoMocker.Get<TMock>());
            if (!this.knownMocks.Contains(mock))
            {
                this.knownMocks.Add(mock);
            }

            mock.Verify(exp, Times.Exactly(times));
        }

        public void VerifyAll() => this.knownMocks.ForEach(x => x.Verify());

        public ISetup<TMock> SetupSet<TMock>(Action<TMock> exp)
            where TMock : class
        {
            var mock = Mock.Get(this.autoMocker.Get<TMock>());
            if (!this.knownMocks.Contains(mock))
            {
                this.knownMocks.Add(mock);
            }

            return mock.SetupSet(exp);
        }

        public TMock Get<TMock>()
            where TMock : class
        {
            var mock = Mock.Get(this.autoMocker.Get<TMock>());
            if (!this.knownMocks.Contains(mock))
            {
                this.knownMocks.Add(mock);
            }

            return mock.Object;
        }

        public void Dispose()
        {
            if (this.target is IDisposable disp)
                disp.Dispose();
        }
    }
}
