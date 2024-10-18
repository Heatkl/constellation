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
    public float ra; // ������ �����������
    public float dec; // ���������
    public float magnitude; // �������
    public string color; // ���� � ������� HEX
}

[System.Serializable]
public class Constellation
{
    public string name;
    public float ra; // ����� ���������
    public float dec;
    public List<Star> stars;
    public List<Pair> pairs; // ������ ����� ����� ��������
}

[System.Serializable]
public class Pair
{
    public int from; // ID ��������� ������
    public int to;   // ID �������� ������
}
