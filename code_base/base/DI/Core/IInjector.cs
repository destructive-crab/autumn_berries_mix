using destruct.ServiceLocators;

namespace destruct.DI;

public interface IInjector
{
    TObject InjectWithBaseAnd<TObject>(TObject toInject, params IServiceLocator[] serviceLocators);
    TObject InjectWithBase<TObject>(TObject toInject);
    TObject InjectWith<TObject>(TObject toInject, params IServiceLocator[] serviceLocators);
}