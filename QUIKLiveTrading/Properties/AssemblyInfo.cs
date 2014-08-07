using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using WealthLab.Extensions.Attribute;

// Управление общими сведениями о сборке осуществляется с помощью 
// набора атрибутов. Измените значения этих атрибутов, чтобы изменить сведения,
// связанные со сборкой.
[assembly: AssemblyTitle("QUIKLiveTrading")]
[assembly: AssemblyDescription("QUIK Static/Streaming Provider with QUIK Broker Provider")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Real-Time Trading Ltd.")]
[assembly: AssemblyProduct("QUIKLiveTrading")]
[assembly: AssemblyCopyright("Copyright © 2013 Real-Time Trading Ltd.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Параметр ComVisible со значением FALSE делает типы в сборке невидимыми 
// для COM-компонентов.  Если требуется обратиться к типу в этой сборке через 
// COM, задайте атрибуту ComVisible значение TRUE для этого типа.
[assembly: ComVisible(false)]

// Следующий GUID служит для идентификации библиотеки типов, если этот проект будет видимым для COM
[assembly: Guid("b3c1cc00-6251-43d4-8257-e7e8c7b11251")]

// Сведения о версии сборки состоят из следующих четырех значений:
//
//      Основной номер версии
//      Дополнительный номер версии 
//      Номер построения:
//            0 - Alpha, X - Ver, XX - Hot Fixes
//            1 - Beta, X - Ver, XX - Hot Fixes
//            2 - RC, X - Ver, XX - Hot Fixes
//            3 - Release, X - SP, XX - Hot Fixes
//      Редакция
//
// Можно задать все значения или принять номер построения и номер редакции по умолчанию, 
// используя "*", как показано ниже:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.3135.160")]
[assembly: AssemblyInformationalVersion("2.0")]

// Сведения о сборке передаваемые в WLD 
[assembly: ExtensionInfo(
    ExtensionType.Provider,
    "QUIKLiveTrading",
    "QUIK Static/Streaming Provider with QUIK Broker Provider",
    "Импорт данных и автоматическая отправка ордеров через терминал QUIK",
    "2.0",
    "Real-Time Trading",
    "WLDSolutions.QUIKLiveTrading.Resources.QUIK[16x16].png",
    ExtensionLicence.Commercial,
    new string[] { "QUIKLiveTrading.dll", "NDde.dll", "TRANS2QUIK.dll" },
    HostApp = ExtensionHostApp.Developer,
    MinDeveloperVersion = "6.4",
    PublisherUrl = @"http://WLDSolutions.ru/")]

#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("QLTUnitTests")]
#endif