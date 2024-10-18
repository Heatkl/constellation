using System.Collections.Generic;


[System.Serializable]
public class ConstellationData
{
    public List<Constellation> items;
}

[System.Serializable]
public class Star
{
    public int id;
    public float ra; // Прямое восхождение
    public float dec; // Склонение
    public float magnitude; // Яркость
    public string color; // Цвет в формате HEX
}

[System.Serializable]
public class Constellation
{
    public string name;
    public float ra; // Центр созвездия
    public float dec;
    public List<Star> stars;
    public List<Pair> pairs; // Список линий между звездами
}

[System.Serializable]
public class Pair
{
    public int from; // ID начальной звезды
    public int to;   // ID конечной звезды
}
