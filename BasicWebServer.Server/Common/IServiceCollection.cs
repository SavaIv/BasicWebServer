using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server.Common
{
    public interface IServiceCollection
    {
        // Тук ще направим методите на inversion Of control контейнер-a ни

        // за да можем да правим чейнинг - трябва да връщаме същия тип!!!
        // затова и така и правим
        IServiceCollection Add<TSercice, TImplementation>()
            where TSercice : class
            where TImplementation : TSercice;
        // Generic методите е добре да имат констрейнти, за да нямаме проблеми!
        // първия констрент казва, че TSercice трябва да е клас (нещо, което да може да се инстанцира). МОЖЕ ДА СЕ ИНСТАНЦИРА!
        // втория констрейнт показва, че трябва да има връзка между TSercice и TImplementation - иначе всичко си губи смисъла
        // по отношение на симантиката на  where TImplementation : TSercice; --> ако първото е интерфейс се чете: "имплементира"
        // ако първото е някаъв клас то ще кажем, че TImplementation "наследява"...

        // Ще имаме само едното от двете на горния метод. причината е, че не винаги ще имаме и интерфейс и имплементация.
        // inversion Of control контейнер-а трябва да поддържа и двата варианта -
        // 1. абстракция, вързана към конкретна имплементация (както е в горния add) и
        // 2. просто конкретна имплементация (т.е. искаме си конкретна инстанция без да я "вързваме" с някакъв интерфейс)
        IServiceCollection Add<TSercice>()
            where TSercice : class;

        // Друг метод, който имаме е ГЕТ, който да ни връща съответния сървис
        TService Get<TService>()
            where TService : class;

        // това което е написано по-долу ще ни трябва по-късно, затова го пишем тук
        object CreateInstance(Type serviceType);



    }
}
