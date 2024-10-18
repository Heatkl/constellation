using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstellationDisplay : MonoBehaviour
{
    public GameObject starPrefab; // Префаб звезды
    public Material lineMaterial; // Материал для линий
    public TextAsset jsonFile; // JSON файл, загружаемый через инспектор
    public float animationDuration = 1.0f; // Время анимации для одной линии

    [SerializeField] private ConstellationData constellations;
    public Button loadJsonButton; // Кнопка загрузки JSON файла

    void Start()
    {
        loadJsonButton.onClick.AddListener(LoadConstellationFromFile);
    }

    void LoadConstellationFromFile()
    {
        // Чтение JSON файла из инспектора
        string jsonContent = jsonFile.text;
        constellations = JsonUtility.FromJson<ConstellationData>(jsonContent);
        StartCoroutine(AnimateConstellation(constellations.items[0]));
    }

    IEnumerator AnimateConstellation(Constellation constellation)
    {
        Dictionary<int, GameObject> starObjects = new Dictionary<int, GameObject>();

        // Создаем звезды
        foreach (var star in constellation.stars)
        {
            Vector3 position = EquatorialToCartesian(star.ra, star.dec);
            GameObject starObject = Instantiate(starPrefab, position, Quaternion.identity);
            starObject.GetComponent<Renderer>().material.color = HexToColor(star.color);
            starObjects[star.id] = starObject;
        }

        // Определяем центральную звезду (ближайшую к центру созвездия)
        int centralStarId = FindCentralStar(constellation.stars, constellation.ra, constellation.dec);

        // Анимируем соединения с использованием BFS
        yield return StartCoroutine(AnimateConnectionsBFS(constellation, centralStarId, starObjects));
    }

    int FindCentralStar(List<Star> stars, float ra, float dec)
    {
        float minDistance = float.MaxValue;
        int centralStarId = -1;
        Vector3 centerPosition = EquatorialToCartesian(ra, dec);

        foreach (var star in stars)
        {
            Vector3 starPosition = EquatorialToCartesian(star.ra, star.dec);
            float distance = Vector3.Distance(centerPosition, starPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                centralStarId = star.id;
            }
        }

        return centralStarId;
    }

    IEnumerator AnimateConnectionsBFS(Constellation constellation, int centralStarId, Dictionary<int, GameObject> starObjects)
    {
        // Используем очередь для BFS
        Queue<int> queue = new Queue<int>();
        HashSet<int> visited = new HashSet<int>();
        queue.Enqueue(centralStarId);
        visited.Add(centralStarId);

        // Карта для поиска пар по ID
        Dictionary<int, List<int>> adjacencyList = new Dictionary<int, List<int>>();

        // Строим список смежностей для каждой звезды
        foreach (var pair in constellation.pairs)
        {
            if (!adjacencyList.ContainsKey(pair.from))
            {
                adjacencyList[pair.from] = new List<int>();
            }
            if (!adjacencyList.ContainsKey(pair.to))
            {
                adjacencyList[pair.to] = new List<int>();
            }
            adjacencyList[pair.from].Add(pair.to);
            adjacencyList[pair.to].Add(pair.from);
        }

        // Анимация по уровням (звезды на одном уровне соединяются одновременно)
        while (queue.Count > 0)
        {
            int levelCount = queue.Count;

            // Для каждой звезды на текущем уровне соединяем её с соседями
            List<IEnumerator> animations = new List<IEnumerator>();

            for (int i = 0; i < levelCount; i++)
            {
                int currentStarId = queue.Dequeue();
                Vector3 currentPosition = starObjects[currentStarId].transform.position;
                try
                {
                    // Соединяем с соседями, которые еще не посещены
                    foreach (var neighbor in adjacencyList[currentStarId])
                {
                    
                        if (!visited.Contains(neighbor))
                        {
                            Vector3 neighborPosition = starObjects[neighbor].transform.position;
                            LineRenderer lineRenderer = CreateLineBetween(currentPosition, neighborPosition);
                            animations.Add(AnimateLine(lineRenderer, currentPosition, neighborPosition));

                            // Добавляем соседа в очередь для последующих соединений
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                        }
                    
                }
                }
                catch { }
            }

            // Запускаем все анимации текущего уровня одновременно
            foreach (var animation in animations)
            {
                yield return StartCoroutine(animation);
            }
        }
    }

    Vector3 EquatorialToCartesian(float ra, float dec)
    {
        // Преобразование экваториальных координат в 3D позиции.
        float x = Mathf.Cos(dec) * Mathf.Cos(ra);
        float y = Mathf.Sin(dec);
        float z = Mathf.Cos(dec) * Mathf.Sin(ra);
        return new Vector3(x, y, z);
    }

    LineRenderer CreateLineBetween(Vector3 start, Vector3 end)
    {
        GameObject lineObject = new GameObject("Line");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, start); // Начинаем линию из точки "start"
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        return lineRenderer;
    }

    IEnumerator AnimateLine(LineRenderer lineRenderer, Vector3 start, Vector3 end)
    {
        float elapsed = 0;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            Vector3 currentPosition = Vector3.Lerp(start, end, t);
            lineRenderer.SetPosition(1, currentPosition);
            yield return null;
        }

        lineRenderer.SetPosition(1, end);
    }

    Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString("#" + hex, out color);
        return color;
    }
}