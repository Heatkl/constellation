using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstellationDisplay : MonoBehaviour
{
    public TextAsset jsonFile; // JSON файл
    public GameObject starPrefab; // Префаб звезды
    public Material lineMaterial; // Материал для линий

    private ConstellationData constellations;

    void Start()
    {
        constellations = JsonUtility.FromJson<ConstellationData>(jsonFile.text);
        StartCoroutine(BuildConstellation(constellations.items[0]));
    }

    void DisplayConstellation(Constellation constellation)
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

        // Соединяем звезды
        foreach (var pair in constellation.pairs)
        {
            if (starObjects.ContainsKey(pair.from) && starObjects.ContainsKey(pair.to))
            {
                CreateLineBetween(starObjects[pair.from].transform.position, starObjects[pair.to].transform.position);
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
        lineRenderer.SetPosition(1, end);
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;

        return lineRenderer;
    }

    Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString("#" + hex, out color);
        return color;
    }

    IEnumerator AnimateLine(LineRenderer lineRenderer, Vector3 start, Vector3 end)
    {
        float duration = 1.0f; // Продолжительность анимации
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 currentPosition = Vector3.Lerp(start, end, t);
            lineRenderer.SetPosition(1, currentPosition);
            yield return null;
        }

        lineRenderer.SetPosition(1, end);
    }

    IEnumerator BuildConstellation(Constellation constellation)
    {
        Dictionary<int, GameObject> starObjects = new Dictionary<int, GameObject>();

        foreach (var star in constellation.stars)
        {
            Vector3 position = EquatorialToCartesian(star.ra, star.dec);
            GameObject starObject = Instantiate(starPrefab, position, Quaternion.identity);
            starObject.GetComponent<Renderer>().material.color = HexToColor(star.color);
            starObjects[star.id] = starObject;
        }

        foreach (var pair in constellation.pairs)
        {
            if (starObjects.ContainsKey(pair.from) && starObjects.ContainsKey(pair.to))
            {
                LineRenderer lineRenderer = CreateLineBetween(starObjects[pair.from].transform.position, starObjects[pair.to].transform.position);
                yield return StartCoroutine(AnimateLine(lineRenderer, starObjects[pair.from].transform.position, starObjects[pair.to].transform.position));
            }
        }
    }
}