using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Mathematics;
using TMPro;

public class GraphHandler : MonoBehaviour
{   
    #region accessible methods

    public void CreatePoint(Vector2 newValue)
    {
        CreatePointInternal(newValue);
    }

    public void ChangePoint(int indexToChange, Vector2 newValue)
    {
        ChangePointInternal(indexToChange, newValue);
    }

    public void SetCornerValues(Vector2 newBottomLeft, Vector2 newTopRight)
    {
        SetCornerValuesInternal(newBottomLeft, newTopRight);
    }

    public void UpdateGraph()
    {
        UpdateGraphInternal(UpdateMethod.All);
    }

    private void ExampleFunction()
    {
        for (float i = 0; i < 50; i += 0.2f)
            CreatePoint(new Vector2(i, 0.2f * i + Mathf.Sin(i)));
        UpdateGraph();
    }
    #endregion

    #region references

    public bool updateGraph = false;
    private GraphSettings GS;
    [SerializeField] private Canvas canvas;
    private RectTransform graph;
    private RectTransform graphContent;

    private Vector2 contentScale = Vector2.zero;
    private List<Vector2> values;
    public List<Vector2> Values{ get {return values; } }
    private List<int> sortedIndices;
    private Vector2Int xAxisRange = new Vector2Int(-1, -1);
    public Vector2Int XAxisRange{ get {return xAxisRange; } }
    private Vector2Int prevXAxisRange = new Vector2Int(-1, -1);
    public int activePointIndex = -1;
    private Vector2 activePointValue = Vector2.zero;
    public Vector2 ActivePointValue{ get {return activePointValue; } }
    private bool pointIsActive = false;
 
    private List<GameObject> points;
    private List<Image> pointImages;
    private List<RectTransform> pointRects;
    private List<GameObject> pointOutlines;
    private List<RectTransform> pointOutlineRects;
    private List<Image> pointOutlineImages;
    private List<GameObject> lines;
    private List<RectTransform> lineRects;
    private List<Image> lineImages;
    private List<RectTransform> xGridRects;
    private List<Image> xGridImages;
    private List<TextMeshProUGUI> xAxisTexts;
    private List<RectTransform> xAxisTextRects;
    private List<RectTransform> yGridRects;
    private List<Image> yGridImages;
    private List<TextMeshProUGUI> yAxisTexts;
    private List<RectTransform> yAxisTextRects;

    private RectTransform zoomSelectionRectTransform;
    private Image zoomSelectionImage;
    private List<RectTransform> zoomSelectionOutlines;
    private List<Image> zoomSelectionOutlineImages;
    private RectTransform pointSelectionRectTransform;
    private Image pointSelectionImage;
    private List<RectTransform> pointSelectionOutlines;
    private List<Image> pointSelectionOutlineImages;

    private RectTransform maskObj;
    private Image backgroundImage;
    private RectTransform backgroundRect;
    private GameObject pointParent;
    private GameObject lineParent;
    private GameObject gridParent;
    private GameObject outlineParent;
    private List<RectTransform> outlines;
    private List<Image> outlineImages;

    private List<int> lockedHoveredPoints;
    private List<int> lockedPoints;
    private List<int> fixedHoveredPoints;
    public int fixedPointIndex = -1;
    private Vector2 contentOffset = Vector2.zero;

    private Vector2 bottomLeft, topRight, center;
    public Vector2 BottomLeft{ get {return bottomLeft; } }
    public Vector2 TopRight{ get {return topRight; } }
    public Vector2 Center{ get {return center; } }
    public enum MouseActionType
    {
        Move,
        SelectAreaToZoom,
        SelectPoints
    }
    public MouseActionType mouseActionType;
    public enum RectangleType
    {
        Free,
        PreserveAspectRatio,
        OriginalAspectRatio
    }
    public RectangleType rectangleType;
    public enum RectangleSelectionType
    {
        SelectAll,
        SelectUnselect
    }
    public RectangleSelectionType rectangleSelectionType;
    public enum RectangleSelectionPhase
    {
        Moving,
        Release
    }
    public RectangleSelectionPhase rectangleSelectionPhase;
    public enum PointSelectionType
    {
        Select,
        FixZoomPoint
    }
    public PointSelectionType pointSelectionType;
    private List<int> initialLockedPoints;
    private List<int> recentlyLockedPoints;
    
    private bool mouseInsideBounds = false;
    private Vector2 mousePos;
    private Vector2 previousMousePos;
    private Vector2 initialMousePos = Vector2.zero;
    private bool initialMouseInsideBounds = false;
    private Vector2 zoomPoint = Vector2.zero;
    private Vector2 absoluteZoomPoint = Vector2.zero;

    public Vector2 targetZoom = new Vector2(1f, 1f);
    private Vector2 zoom = new Vector2(1f, 1f);
    
    private Vector2 moveOffset;
    public Vector2 targetMoveOffset;
    private Vector2 initialMoveOffset = Vector2.zero;

    private float timeToUpdateMouse = 0;
    private float timeToUpdateTouch = 0;
    private float timeToUpdateScroll = 0;
    private bool error;

    private void Awake()
    {
        values = new List<Vector2>();
        sortedIndices = new List<int>();
        points = new List<GameObject>();
        pointRects = new List<RectTransform>();
        pointImages = new List<Image>();
        pointOutlines = new List<GameObject>();
        pointOutlineRects = new List<RectTransform>();
        pointOutlineImages = new List<Image>();
        lines = new List<GameObject>();
        lineRects = new List<RectTransform>();
        lineImages = new List<Image>();
        xGridRects = new List<RectTransform>();
        xGridImages = new List<Image>();
        yGridRects = new List<RectTransform>();
        yGridImages = new List<Image>();
        xAxisTexts = new List<TextMeshProUGUI>();
        yAxisTexts = new List<TextMeshProUGUI>();
        xAxisTextRects = new List<RectTransform>();
        yAxisTextRects = new List<RectTransform>();
        zoomSelectionOutlines = new List<RectTransform>();
        zoomSelectionOutlineImages = new List<Image>();
        pointSelectionOutlines = new List<RectTransform>();
        pointSelectionOutlineImages = new List<Image>();
        outlines = new List<RectTransform>();
        outlineImages = new List<Image>();
        lockedHoveredPoints = new List<int>();
        lockedPoints = new List<int>();
        initialLockedPoints = new List<int>();
        recentlyLockedPoints = new List<int>();
        fixedHoveredPoints = new List<int>();
    }

    #endregion

    #region private methods

    private void Start()
    {
        if(CheckForErrors())
            return;
        GS = GetComponent<GraphSettings>(); 
        PrepareGraph();
        ExampleFunction();
    }
    private void Update()
    {
        if(Input.GetKey(KeyCode.LeftShift))
            mouseActionType = MouseActionType.SelectAreaToZoom;
        else if(Input.GetKey(KeyCode.LeftControl))
            mouseActionType = MouseActionType.SelectPoints;
        else
            mouseActionType = MouseActionType.Move;
        if(error)
            return; 
        CheckIfUpdateGraph();
        /*
        for(int i = 0; i <= xAxisRange.y + 1; i += 2)
        {
            if(xAxisRange.x != -1 && xAxisRange.y != -1)
            {
                int index = sortedIndices[i];
                values[index] = new Vector2(values[index].x, Mathf.Sin(Time.time / 2f + values[index].x));
                lineImages[index].color = new Color(1f, Mathf.Sin(Time.time * 2f + values[index].x) / 2f + 0.35f, 0, 1f);
            }
        }
        for(int i = 1; i <= xAxisRange.y; i += 2)
        {
            if(xAxisRange.x != -1 && xAxisRange.y != -1)
            {
                int index = sortedIndices[i];
                values[index] = new Vector2(values[index].x, -Mathf.Sin(Time.time / 2f + values[index].x));
                lineImages[index].color = new Color(1f, Mathf.Sin(Time.time * 2f + values[index].x) / 2f + 0.35f, 0, 1f);
            }
        }*/
        if(updateGraph)
            UpdateGraphInternal(UpdateMethod.All);
        
        if(lockedHoveredPoints.Count > 0)
            UpdatePoints();
        if(fixedHoveredPoints.Count > 0)
            UpdatePointOutlines();
        zoom = Vector2.Lerp(zoom, targetZoom, GS.SmoothZoomSpeed * Time.deltaTime);
        moveOffset = Vector2.Lerp(moveOffset, targetMoveOffset, GS.SmoothMoveSpeed * Time.deltaTime);
    }
    private void PrepareGraph()
    {
        if(canvas == null)
        {
            canvas = new GameObject("GraphCanvas").AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        graph = this.gameObject.AddComponent<RectTransform>();
        graph.SetParent(canvas.transform);
        graph.anchoredPosition = Vector2.zero;
        graph.sizeDelta = GS.GraphSize;

        maskObj = new GameObject("MaskObj").AddComponent<RectTransform>();
        maskObj.SetParent(graph);
        maskObj.anchoredPosition = Vector2.zero;
        maskObj.gameObject.AddComponent<Image>();
        Mask mask = maskObj.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        backgroundRect = new GameObject("Background").AddComponent<RectTransform>();
        backgroundRect.SetParent(maskObj);
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundImage = backgroundRect.gameObject.AddComponent<Image>();

        graphContent = new GameObject("GraphContent").AddComponent<RectTransform>();
        graphContent.SetParent(backgroundRect.transform);
        graphContent.sizeDelta = Vector2.zero;

        gridParent = CreateParent("GridParent");
        lineParent = CreateParent("LineParent");
        pointParent = CreateParent("PointParent");

        outlineParent = CreateParent("OutlineParent");
        CreateOutlines();
        outlineParent.transform.SetParent(graph);
        
        fixedPointIndex = -1;
        //SetCornerValues(Vector2.zero, new Vector2(3f, 3f * GS.GraphSize.y / GS.GraphSize.x));
        CreateselectionTypes();
        UpdateGraphInternal(UpdateMethod.All);
    }

    private GameObject CreateParent(string name)
    {
        GameObject parent = new GameObject(name);
        parent.transform.SetParent(name == "OutlineParent" ? graph : graphContent);
        Image image = parent.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0);
        image.raycastTarget = false;
        parent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        return parent;
    }

    private void CreateOutlines()
    {
        for(int i = 0; i < 4; i++)
        {
            Image outlineImage = new GameObject("Outline").AddComponent<Image>();
            RectTransform outline = outlineImage.GetComponent<RectTransform>();
            outline.SetParent(outlineParent.transform);
            outlineImage.color = GS.OutlineColor;
            outlineImage.raycastTarget = false;
            outlines.Add(outline);
            outlineImages.Add(outlineImage);
        }
    }

    private void CreatePointInternal(Vector2 value)
    {
        
        int i = points.Count;
        GameObject outline = CreatePointOutline(i);
        GameObject point = new GameObject("Point" + i);

        points.Add(point);
        values.Add(value);

        point.transform.SetParent(outline.transform);

        Image image = point.AddComponent<Image>();
        image.color = GS.PointColor;
        pointImages.Add(image);
        RectTransform pointRectTransform = point.GetComponent<RectTransform>();
        pointRectTransform.sizeDelta = Vector2.one * GS.PointRadius;
        pointRects.Add(pointRectTransform);
        image.sprite = GS.PointSprite;

        EventTrigger trigger = point.AddComponent<EventTrigger>();
        var eventTypes = new[]
        {
            new { Type = EventTriggerType.PointerEnter, Callback = (Action) (() => MouseTrigger(i, true)) },
            new { Type = EventTriggerType.PointerExit, Callback = (Action) (() => MouseTrigger(i, false)) },
            new { Type = EventTriggerType.PointerClick, Callback = (Action) (() => PointClicked(i)) }
        };
        foreach (var eventType in eventTypes)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType.Type };
            entry.callback.AddListener((data) => { eventType.Callback(); });
            trigger.triggers.Add(entry);
        }
        if (points.Count > 1)
        {
            GameObject line = new GameObject("Line");
            line.transform.SetParent(lineParent.transform);
            lineImages.Add(line.AddComponent<Image>());
            line.GetComponent<Image>().color = GS.LineColor;
            lineRects.Add(line.GetComponent<RectTransform>());
            lines.Add(line);
            line.SetActive(false);
        }
        lockedHoveredPoints.Add(i);
        SortIndices();

        outline.SetActive(false);
    }

    private void ChangePointInternal(int index, Vector2 newValue)
    {
        values[index] = newValue;
        SortIndices();
    }

    private void SortIndices()
    {
        sortedIndices = values.Select((vector, index) => new { vector, index })
            .OrderBy(item => item.vector.x)
            .ThenBy(item => item.vector.y)
            .Select(item => item.index)
            .ToList();
    }


    private GameObject CreatePointOutline(int i)
    {
        GameObject outline = new GameObject("PointOutline" + i);
        pointOutlines.Add(outline);

        if (pointParent != null)
        {
            outline.transform.SetParent(pointParent.transform);
        }

        Image image = outline.AddComponent<Image>();
        image.color = GS.PointColor;
        pointOutlineImages.Add(image);
        RectTransform rectTransform = outline.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(GS.PointRadius, GS.PointRadius);
        pointOutlineRects.Add(rectTransform);

        Sprite sprite = GS.PointSprite;
        image.sprite = sprite;

        return outline;
    }

    private void CreateGridLines(bool createX)
    {
        if(createX)
        {
            GameObject xGrid = new GameObject("xGrid" + xGridRects.Count);
            xGrid.transform.SetParent(gridParent.transform);
            Image xGridImage = xGrid.AddComponent<Image>();
            xGridImage.raycastTarget = false;
            xGridRects.Add(xGrid.GetComponent<RectTransform>());
            xGridImages.Add(xGridImage);
            if(xGridRects.Count > 1)
            {
                TextMeshProUGUI xText = new GameObject("xText" + xGridRects.Count).AddComponent<TextMeshProUGUI>();
                RectTransform textRect = xText.gameObject.GetComponent<RectTransform>();
                textRect.SetParent(xGrid.GetComponent<RectTransform>());
                xText.font = GS.GridTextFont;
                xText.fontStyle = FontStyles.Bold;
                xText.fontStyle = FontStyles.Bold;
                xText.alignment = TextAlignmentOptions.Center;
                xText.verticalAlignment = VerticalAlignmentOptions.Middle;
                xText.color = GS.XAxisTextColor;
                xText.enableAutoSizing = true;
                textRect.sizeDelta = Vector2.one * GS.XAxisTextSize;
                xText.raycastTarget = false;
                xAxisTexts.Add(xText);
                xAxisTextRects.Add(textRect);
            }
        }
        else
        {
            GameObject yGrid = new GameObject("yGrid" + yGridRects.Count);
            yGrid.transform.SetParent(gridParent.transform);
            Image yGridImage = yGrid.AddComponent<Image>();
            yGridImage.raycastTarget = false;
            yGridRects.Add(yGrid.GetComponent<RectTransform>());
            yGridImages.Add(yGridImage);
            if(yGridRects.Count > 1)
            {
                TextMeshProUGUI yText = new GameObject("yText" + yGridRects.Count).AddComponent<TextMeshProUGUI>();
                RectTransform textRect = yText.gameObject.GetComponent<RectTransform>();
                textRect.SetParent(yGrid.GetComponent<RectTransform>());
                yText.font = GS.GridTextFont;
                yText.fontStyle = FontStyles.Bold;
                yText.alignment = TextAlignmentOptions.Center;
                yText.verticalAlignment = VerticalAlignmentOptions.Middle;
                yText.color = GS.YAxisTextColor;
                yText.enableAutoSizing = true;
                textRect.sizeDelta = Vector2.one * GS.YAxisTextSize;
                yText.raycastTarget = false;
                yAxisTexts.Add(yText);
                yAxisTextRects.Add(textRect);
            }
            
        }
    }

    private void CreateselectionTypes()
    {
        GameObject selectionParent = new GameObject("SelectionParent");
        selectionParent.transform.SetParent(graphContent);
        selectionParent.AddComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
        zoomSelectionImage = new GameObject("ZoomSelection").AddComponent<Image>();
        zoomSelectionRectTransform = zoomSelectionImage.GetComponent<RectTransform>();
        zoomSelectionRectTransform.SetParent(selectionParent.transform);
        for(int i = 0; i < 4; i++)
        {
            Image image = new GameObject("Outline").AddComponent<Image>();
            RectTransform rect = image.GetComponent<RectTransform>();
            rect.SetParent(zoomSelectionRectTransform);
            zoomSelectionOutlineImages.Add(image);
            zoomSelectionOutlines.Add(rect);
        }
        zoomSelectionRectTransform.gameObject.SetActive(false);
        pointSelectionImage = new GameObject("PointSelection").AddComponent<Image>();
        pointSelectionRectTransform = pointSelectionImage.GetComponent<RectTransform>();
        pointSelectionRectTransform.SetParent(selectionParent.transform);
        for(int i = 0; i < 4; i++)
        {
            Image image = new GameObject("Outline").AddComponent<Image>();
            RectTransform rect = image.GetComponent<RectTransform>();
            rect.SetParent(pointSelectionRectTransform);
            pointSelectionOutlineImages.Add(image);
            pointSelectionOutlines.Add(rect);
        }
        pointSelectionRectTransform.gameObject.SetActive(false);
    }

    private void CheckIfUpdateGraph()
    {   
        CalculateMousePosition();
        if(mouseInsideBounds)
        {
            if(Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
            {
                timeToUpdateMouse = GS.updatePeriod;
            }
            if(Input.touchCount > 0)
            {
                timeToUpdateTouch = GS.updatePeriod;
                
            }
            if(Input.mouseScrollDelta.y != 0)
            {
                timeToUpdateScroll = GS.updatePeriod;
            }
        }
        if(timeToUpdateMouse > 0)
            UpdateGraphInternal(UpdateMethod.UpdatePositionAndScale | UpdateMethod.UpdatePointVisuals | UpdateMethod.UpdateContent | UpdateMethod.MouseAction | UpdateMethod.UpdateGridLines);
        if(timeToUpdateTouch > 0)
            UpdateGraphInternal(UpdateMethod.UpdatePositionAndScale | UpdateMethod.UpdatePointVisuals | UpdateMethod.UpdateContent | UpdateMethod.MouseZoom | UpdateMethod.MouseAction | UpdateMethod.UpdateGridLines);
        if(timeToUpdateScroll > 0)
            UpdateGraphInternal(UpdateMethod.UpdatePositionAndScale | UpdateMethod.UpdatePointVisuals | UpdateMethod.UpdateContent | UpdateMethod.MouseZoom | UpdateMethod.UpdateGridLines);
        timeToUpdateMouse -= Time.deltaTime;
        timeToUpdateTouch -= Time.deltaTime;
        timeToUpdateScroll -= Time.deltaTime;
    }
    public void UpdateGraphInternal(UpdateMethod methodsToUpdate)
    {
        if (methodsToUpdate.HasFlag(UpdateMethod.UpdatePositionAndScale) || methodsToUpdate.HasFlag(UpdateMethod.All))
            UpdatePositionAndScale();
        CalculateCornerValues();
        if (methodsToUpdate.HasFlag(UpdateMethod.UpdateOutlines) || methodsToUpdate.HasFlag(UpdateMethod.All))
            UpdateOutlines();
        if (methodsToUpdate.HasFlag(UpdateMethod.UpdateContent) || methodsToUpdate.HasFlag(UpdateMethod.All))
            HandleActiveObjects();
        if (methodsToUpdate.HasFlag(UpdateMethod.UpdatePointVisuals) || methodsToUpdate.HasFlag(UpdateMethod.All))
            UpdatePointVisuals();
        if (methodsToUpdate.HasFlag(UpdateMethod.UpdateContent) || methodsToUpdate.HasFlag(UpdateMethod.All))
            UpdateContent();
        CalculateMousePosition();
        if (methodsToUpdate.HasFlag(UpdateMethod.MouseZoom) || methodsToUpdate.HasFlag(UpdateMethod.All))
            MouseZoom();
        if (methodsToUpdate.HasFlag(UpdateMethod.MouseAction) || methodsToUpdate.HasFlag(UpdateMethod.All))
            MouseAction();
        if (methodsToUpdate.HasFlag(UpdateMethod.UpdateGridLines) || methodsToUpdate.HasFlag(UpdateMethod.All))
            UpdateGridLines();
    }
    private void UpdatePositionAndScale()
    {
        contentScale = GS.GraphScale * zoom;
        maskObj.sizeDelta = GS.GraphSize;
        contentOffset = absoluteZoomPoint - zoomPoint * contentScale - moveOffset;
        graphContent.anchoredPosition = -GS.GraphSize / 2 + contentOffset;
        graph.sizeDelta = GS.GraphSize;
        backgroundRect.sizeDelta = GS.GraphSize;
        backgroundImage.color = GS.BackgroundColor;
    }
    private void UpdateOutlines()
    {
        for (int i = 0; i < outlines.Count; i++)
        {
            if (i % 2 == 0) // Left and Right outlines
            {
                outlines[i].sizeDelta = new Vector2(GS.OutlineWidth, GS.GraphSize.y + GS.OutlineWidth * 2);
                outlines[i].anchoredPosition = new Vector2((i == 0 ? -1 : 1) * (GS.GraphSize.x + GS.OutlineWidth) / 2, 0);
            }
            else // Top and Bottom outlines
            {
                outlines[i].sizeDelta = new Vector2(GS.GraphSize.x + GS.OutlineWidth * 2, GS.OutlineWidth);
                outlines[i].anchoredPosition = new Vector2(0, (i == 1 ? -1 : 1) * (GS.GraphSize.y + GS.OutlineWidth) / 2);
            }
            outlineImages[i].color = GS.OutlineColor;
        }
    }
    private void CalculateCornerValues()
    {
        topRight = new Vector2(Mathf.Clamp(topRight.x, bottomLeft.x, Mathf.Infinity), Mathf.Clamp(topRight.y, bottomLeft.y, Mathf.Infinity));
        bottomLeft = -contentOffset / contentScale;
        topRight = bottomLeft + GS.GraphSize / contentScale;
        center = (topRight - bottomLeft) / 2f + bottomLeft;
    }
    private void UpdateContent()
    {
        if (xAxisRange.x == -1 || xAxisRange.y == -1)
            return;
        Vector2 bounds = new Vector2(bottomLeft.y, topRight.y);
        for (int i = xAxisRange.x - 1; i <= xAxisRange.y + 1; i++)
        {
            if (i < 0 || i > sortedIndices.Count - 1)
                continue;
            int index = sortedIndices[i];
            float currentValue = values[index].y;
            float prevValue = values[Mathf.Clamp(index - 1, 0, values.Count - 1)].y;
            float nextValue = values[Mathf.Clamp(index + 1, 0, values.Count - 1)].y;

            if ((currentValue < bounds.x && prevValue < bounds.x && nextValue < bounds.x) ||
                (currentValue > bounds.y && prevValue > bounds.y && nextValue > bounds.y))
            {
                continue;
            }
            UpdateAnchoredPosition(pointOutlineRects[index], CalculatePosition(i));
            if (lines.Count > 0 && index < lines.Count)
            {
                Vector2 point1 = CalculatePosition(index);
                Vector2 point2 = CalculatePosition(index + 1);
                float distance = Vector2.Distance(point1, point2);
                UpdateAnchoredPosition(lineRects[index], (point2 + point1) / 2f);
                UpdateSizeDelta(lineRects[index], new Vector2(distance, GS.LineWidth));
                Vector2 direction = point2 - point1;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                lineRects[index].rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                lineImages[index].color = GS.LineColor;
            }
        }
    }
    private void HandleActiveObjects()
    {
        if(prevXAxisRange.x < xAxisRange.x)
        {
            for(int i = prevXAxisRange.x - 1; i < xAxisRange.x - 1; i++)
            {
                if(i < 0)
                    continue;
                pointOutlines[sortedIndices[i]].SetActive(false);
                if(i < lines.Count)
                    lines[sortedIndices[i]].SetActive(false);
            }
        }
        else if(prevXAxisRange.x > xAxisRange.x && xAxisRange.x >= 0)
        {
            for(int i = xAxisRange.x - 1; i < prevXAxisRange.x; i++)
            {
                if(i < 0)
                    continue;
                pointOutlines[sortedIndices[i]].SetActive(true);
                if(i < lines.Count)
                    lines[sortedIndices[i]].SetActive(true);
            }
        }
        if(prevXAxisRange.y > xAxisRange.y)
        {
            for(int i = xAxisRange.y + 2; i <= prevXAxisRange.y + 2; i++)
            {
                if(i > pointOutlines.Count - 1 || i < 0)
                    continue;
                pointOutlines[sortedIndices[i]].SetActive(false);
                if(i < lines.Count)
                    lines[sortedIndices[i]].SetActive(false);
            }
        }
        else if(xAxisRange.y > prevXAxisRange.y)
        {
            for(int i = prevXAxisRange.y + 2; i <= xAxisRange.y + 1; i++)
            {
                if(i > pointOutlines.Count - 1 || i < 0)
                    continue;
                pointOutlines[sortedIndices[i]].SetActive(true);
                if(i < lines.Count)
                    lines[sortedIndices[i]].SetActive(true);
            }
        }
        prevXAxisRange = xAxisRange;
        xAxisRange = new Vector2Int(MinMaxBinarySearch(true), MinMaxBinarySearch(false));
    }
    private Vector2 CalculatePosition(int i)
    {
        return values[i] * contentScale;
    }
    private void MouseTrigger(int pointIndex, bool enter)
    {
        fixedHoveredPoints.Add(pointIndex);
        fixedHoveredPoints = fixedHoveredPoints.Distinct().ToList();

        if (enter)
        {
            activePointIndex = pointIndex;
            activePointValue = values[pointIndex];
            pointIsActive = enter;

            if (pointSelectionType == PointSelectionType.Select)
            {
                lockedHoveredPoints.Add(pointIndex);
                lockedHoveredPoints = lockedHoveredPoints.Distinct().ToList();
            }
        }
        else
        {
            // Do not process PointerExit event for locked points
            if (lockedPoints.Contains(activePointIndex) && pointSelectionType == PointSelectionType.Select || fixedPointIndex == activePointIndex && pointSelectionType == PointSelectionType.FixZoomPoint)
            {
                return;
            }

            activePointIndex = pointIndex;
            activePointValue = values[pointIndex];
            pointIsActive = enter;
        }
    }
    private void PointClicked(int pointIndex)
    {
        if (pointSelectionType == PointSelectionType.FixZoomPoint)
        {
            if (fixedPointIndex != -1)
            {
                ChangeZoomPoint(values[pointIndex]);
                fixedHoveredPoints.Add(fixedPointIndex);
            }
    
            fixedHoveredPoints = fixedHoveredPoints.Distinct().ToList();
    
            fixedPointIndex = (fixedPointIndex == pointIndex) ? -1 : pointIndex;
            ChangeZoomPoint((fixedPointIndex == -1) ? center : values[pointIndex]);
        }
        else
        {
            if (lockedPoints.Contains(pointIndex))
            {
                lockedPoints.Remove(pointIndex);
            }
            else
            {
                lockedPoints.Add(pointIndex);
            }
        }
    }
    private void UpdatePointVisuals()
    {
        if(xAxisRange.x == -1 || xAxisRange.y == -1)
            return;
        for(int i = xAxisRange.x; i <= xAxisRange.y; i++)
        {
            if(activePointIndex == sortedIndices[i])
                continue;
            lockedHoveredPoints.Add(sortedIndices[i]);
            fixedHoveredPoints.Add(sortedIndices[i]);
        }
    }
    private void UpdatePoints()
    {
        for (int i = 0; i < lockedHoveredPoints.Count; i++)
        {
            Vector2 targetSize;
            Color targetColor;
            float targetSpeed;
            int numToUpdate = lockedHoveredPoints[i];
            bool isActive = activePointIndex == numToUpdate && pointIsActive;

            if (lockedPoints.Contains(numToUpdate))
            {
                targetSize = Vector2.one * GS.PointLockedRadius;
                targetColor = GS.PointLockedColor;
                targetSpeed = GS.PointLockedSpeed;
            }
            else if (isActive && pointSelectionType == PointSelectionType.Select)
            {
                targetSize = Vector2.one * GS.PointHoverRadius;
                targetColor = GS.PointHoverColor;
                targetSpeed = GS.PointHoverSpeed;
            }
            else
            {
                targetSize = Vector2.one * GS.PointRadius;
                targetColor = GS.PointColor;
                targetSpeed = GS.PointHoverSpeed;
            }

            pointRects[numToUpdate].sizeDelta = Vector2.Lerp(pointRects[numToUpdate].sizeDelta, targetSize, Time.deltaTime * targetSpeed);
            pointImages[numToUpdate].color = Color.Lerp(pointImages[numToUpdate].color, targetColor, Time.deltaTime * targetSpeed);

            if (!isActive && Vector2.Distance(pointRects[numToUpdate].sizeDelta, targetSize) < 0.5f && Vector4.Distance(pointImages[numToUpdate].color, targetColor) < 0.5f)
            {
                pointImages[numToUpdate].color = targetColor;
                pointRects[numToUpdate].sizeDelta = targetSize;
                lockedHoveredPoints.RemoveAt(i);
            }
            else
            {
                fixedHoveredPoints.Add(numToUpdate);
                fixedHoveredPoints = fixedHoveredPoints.Distinct().ToList();
            }
        }
        UpdatePointOutlines();
    }
    private void UpdatePointOutlines()
    {
        for (int i = 0; i < fixedHoveredPoints.Count; i++)
        {
            Vector2 targetSize;
            Color targetColor;
            float targetSpeed;
            int numToUpdate = fixedHoveredPoints[i];
            bool isActive = activePointIndex == numToUpdate && pointIsActive;

            if (fixedPointIndex == numToUpdate)
            {
                targetSize = new Vector2(GS.FixedPointOutlineWidth, GS.FixedPointOutlineWidth);
                targetColor = GS.FixedPointOutlineColor;
                targetSpeed = GS.FixedPointOutlineSpeed;
            }
            else if (isActive && pointSelectionType == PointSelectionType.FixZoomPoint)
            {
                targetSize = new Vector2(GS.UnfixedPointOutlineHoverWidth, GS.UnfixedPointOutlineHoverWidth);
                targetColor = GS.UnfixedPointOutlineHoverColor;
                targetSpeed = GS.UnfixedPointOutlineHoverSpeed;
            }
            else
            {
                targetSize = new Vector2(GS.UnfixedPointOutlineWidth, GS.UnfixedPointOutlineWidth);
                targetColor = GS.UnfixedPointOutlineColor;
                targetSpeed = GS.UnfixedPointOutlineHoverSpeed;
            }

            targetSize += pointRects[numToUpdate].sizeDelta;
            RectTransform outlineRectTransform = pointOutlineRects[numToUpdate];

            if (pointSelectionType == PointSelectionType.FixZoomPoint)
                outlineRectTransform.sizeDelta = Vector2.Lerp(outlineRectTransform.sizeDelta, targetSize, Time.deltaTime * targetSpeed);
            else
                outlineRectTransform.sizeDelta = targetSize;

            Image outlineImage = pointOutlineImages[numToUpdate];
            targetColor = Color.Lerp(outlineImage.color, targetColor, Time.deltaTime * targetSpeed);
            outlineImage.color = targetColor;

            if (!isActive && Vector2.Distance(outlineRectTransform.sizeDelta, targetSize) < 0.5f)
                fixedHoveredPoints.RemoveAt(i);
        }
    }

    private void UpdateGridLines()
    {
        Vector2 GridStartPoint;
        Vector2 spacing = CalculateGridSpacing();
        GridStartPoint = new Vector2(Mathf.Ceil(bottomLeft.x * spacing.x) / spacing.x, Mathf.Ceil(bottomLeft.y * spacing.y) / spacing.y) * contentScale;
        int2 eventualOverlay = new int2(-1, -1);
        int requiredYGridlines = Mathf.CeilToInt((topRight.y - bottomLeft.y) * spacing.y) + 1;
        int requiredXGridlines = Mathf.CeilToInt((topRight.x - bottomLeft.x) * spacing.x) + 1;
        while(xGridRects.Count <= requiredXGridlines)
        {
            CreateGridLines(true);
        }
        while(yGridRects.Count <= requiredYGridlines)
        {
            CreateGridLines(false);
        }

        for (int i = 0; i < requiredXGridlines; i++)
        {
            RectTransform rect = xGridRects[i];
            Image rectImage = xGridImages[i];
            if(!rect.gameObject.activeSelf)
                rect.gameObject.SetActive(true);
            if (i == 0)
            {
                UpdateSizeDelta(rect, new Vector2(GS.XAxisWidth, GS.GraphSize.y * 2f));
                rectImage.color = GS.XAxisColor;
                UpdateAnchoredPosition(rect, new Vector2(0, center.y * contentScale.y));
            }
            else
            {
                UpdateSizeDelta(rect, new Vector2(GS.XGridWidth, GS.GraphSize.y * 2f));
                rectImage.color = GS.XGridColor;

                if (Mathf.Round(GridStartPoint.x + (i + eventualOverlay.x) / spacing.x * contentScale.x) == 0)
                    eventualOverlay.x = 0;

                UpdateAnchoredPosition(rect, new Vector2(GridStartPoint.x + (i + eventualOverlay.x) / spacing.x * contentScale.x, center.y * contentScale.y));
                UpdateSizeDelta(xAxisTextRects[i - 1], new Vector2(1f / spacing.x * contentScale.x, GS.XAxisTextSize));
                UpdateAnchoredPosition(xAxisTextRects[i - 1], new Vector2(0, -center.y * contentScale.y + GS.XAxisTextOffset));
                xAxisTexts[i - 1].text = Mathf.Floor(1f / spacing.x) > 0 ? Mathf.RoundToInt(GridStartPoint.x / contentScale.x + (i + eventualOverlay.x) / spacing.x).ToString() : (GridStartPoint.x / contentScale.x + (i + eventualOverlay.x) / spacing.x).ToString("R");
            }
        }

        for (int i = 0; i < requiredYGridlines; i++)
        {
            RectTransform rect = yGridRects[i];
            Image rectImage = yGridImages[i];
            if(!rect.gameObject.activeSelf)
                rect.gameObject.SetActive(true);
            if (i == 0)
            {
                UpdateSizeDelta(rect, new Vector2(GS.GraphSize.x * 2f, GS.YAxisWidth));
                rectImage.color = GS.YAxisColor;
                UpdateAnchoredPosition(rect, new Vector2(center.x * contentScale.x, 0));
            }
            else
            {
                UpdateSizeDelta(rect, new Vector2(GS.GraphSize.x * 2f, GS.YGridWidth));
                rectImage.color = GS.YGridColor;

                if (Mathf.Round(GridStartPoint.y + (i + eventualOverlay.y) / spacing.y * contentScale.y) == 0)
                    eventualOverlay.y = 0;

                UpdateAnchoredPosition(rect, new Vector2(center.x * contentScale.x, GridStartPoint.y + (i + eventualOverlay.y) / spacing.y * contentScale.y));
                UpdateSizeDelta(yAxisTextRects[i - 1], new Vector2(1f / spacing.x * contentScale.x, GS.XAxisTextSize));
                UpdateAnchoredPosition(yAxisTextRects[i - 1], new Vector2(-center.x * contentScale.x + GS.YAxisTextOffset, 0));
                yAxisTexts[i - 1].text = Mathf.Floor(1f / spacing.y) > 0 ? Mathf.RoundToInt(GridStartPoint.y / contentScale.y + (i + eventualOverlay.y) / spacing.y).ToString() : (GridStartPoint.y / contentScale.y + (i + eventualOverlay.y) / spacing.y).ToString("R");
            }
        }
        for(int i = requiredXGridlines; i < xGridRects.Count; i++)
        {
            if(xGridRects[i].gameObject.activeSelf)
                xGridRects[i].gameObject.SetActive(false);
        }
        for(int i = requiredYGridlines; i < yGridRects.Count; i++)
        {
            if(yGridRects[i].gameObject.activeSelf)
                yGridRects[i].gameObject.SetActive(false);
        }
    }
    private Vector2 CalculateGridSpacing()
    {
        int exponentX = Mathf.FloorToInt(Mathf.Log(zoom.x, 2));
        int exponentY = Mathf.FloorToInt(Mathf.Log(zoom.y, 2));
        float closestX = Mathf.Pow(2, exponentX);
        float closestY = Mathf.Pow(2, exponentY);
        return new Vector2(closestX, closestY) * GS.GridSpacing;
    }
    private void SetCornerValuesInternal(Vector2 newBottomLeft, Vector2 newTopRight)
    {
        Vector2 newCenter = (newTopRight - newBottomLeft) / 2f + newBottomLeft;
        targetMoveOffset = (newCenter - center) * contentScale + moveOffset;

        ChangeZoomPoint(newCenter);
        targetZoom = GS.GraphSize / GS.GraphScale / (newTopRight - newBottomLeft);
    }
    private void CalculateMousePosition()
    {
        mousePos = (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - new Vector2(graphContent.transform.position.x, graphContent.transform.position.y)) / contentScale;
        mouseInsideBounds = mousePos.x > bottomLeft.x && mousePos.y > bottomLeft.y && mousePos.x < topRight.x && mousePos.y < topRight.y;
        mousePos = new Vector2(Mathf.Clamp(mousePos.x, bottomLeft.x, topRight.x), Mathf.Clamp(mousePos.y, bottomLeft.y, topRight.y));
    }
    private void MouseZoom()
    {
        if(!mouseInsideBounds)
            return;
        if(Input.mouseScrollDelta.y == 0)
            return;
        if(fixedPointIndex == -1)
            ChangeZoomPoint(mousePos);
        targetZoom = zoom + Input.mouseScrollDelta.y * zoom * GS.ZoomSpeed / 100f;
    }
    private void ChangeZoomPoint(Vector2 newZoomPoint)
    {
        absoluteZoomPoint = (newZoomPoint -zoomPoint) * contentScale + absoluteZoomPoint;
        zoomPoint = newZoomPoint;
    }

    private void MouseAction()
    {
        if(Input.GetMouseButtonDown(0))
        {
            initialMouseInsideBounds = mouseInsideBounds;
            if(!mouseInsideBounds)
                return;
            
            initialMousePos = Input.mousePosition;
            if(mouseActionType == MouseActionType.Move)
                initialMoveOffset = moveOffset;
            else if(mouseActionType == MouseActionType.SelectPoints)
            {
                initialLockedPoints.Clear();
                for(int i = 0; i < lockedPoints.Count; i++)
                    initialLockedPoints.Add(lockedPoints[i]);
            }
            return;
        }
        if(Input.GetMouseButton(0) && initialMouseInsideBounds)
        {
            if(Input.GetMouseButtonDown(1))
            {
                initialMouseInsideBounds = false;
                zoomSelectionRectTransform.gameObject.SetActive(false);
                pointSelectionRectTransform.gameObject.SetActive(false);
            }
            if(previousMousePos != mousePos)
            {
                Vector2 currentMousePos = Input.mousePosition;
                if(mouseActionType == MouseActionType.Move)
                    targetMoveOffset = (initialMousePos - currentMousePos) + initialMoveOffset;
                else if(mouseActionType == MouseActionType.SelectAreaToZoom)
                {
                    if(!zoomSelectionRectTransform.gameObject.activeSelf)
                        zoomSelectionRectTransform.gameObject.SetActive(true);
                    SelectAreaToZoom(false);
                }
                else if (mouseActionType == MouseActionType.SelectPoints)
                {
                    if(!pointSelectionRectTransform.gameObject.activeSelf)
                        pointSelectionRectTransform.gameObject.SetActive(true);
                    SelectPoints(false);
                }
            }
            previousMousePos = mousePos;
        }
        else if(Input.GetMouseButtonUp(0) && initialMouseInsideBounds)
        {
            if(mouseActionType == MouseActionType.SelectAreaToZoom)
                SelectAreaToZoom(true);
            else if(mouseActionType == MouseActionType.SelectPoints)
                SelectPoints(true);
            recentlyLockedPoints.Clear();
        }
        
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began)
            {
                initialMousePos = touch.position;
                mousePos = (new Vector2(touch.position.x, touch.position.y) - new Vector2(graphContent.transform.position.x, graphContent.transform.position.y)) / contentScale;
                initialMouseInsideBounds = mousePos.x > bottomLeft.x && mousePos.y > bottomLeft.y && mousePos.x < topRight.x && mousePos.y < topRight.y;
                if(mouseActionType == MouseActionType.Move)
                    initialMoveOffset = moveOffset;
                else if(mouseActionType == MouseActionType.SelectPoints)
                {
                    initialLockedPoints.Clear();
                    for(int i = 0; i < lockedPoints.Count; i++)
                        initialLockedPoints.Add(lockedPoints[i]);
                }
                    
            }
            else if(touch.phase == TouchPhase.Moved && initialMouseInsideBounds)
            {
                if(mouseActionType == MouseActionType.Move)
                {
                    Vector2 currentTouchPos = touch.position;
                    targetMoveOffset = (initialMousePos - currentTouchPos) + initialMoveOffset;
                }
                else if(mouseActionType == MouseActionType.SelectAreaToZoom)
                {
                    if(!zoomSelectionRectTransform.gameObject.activeSelf)
                        zoomSelectionRectTransform.gameObject.SetActive(true);
                    SelectAreaToZoom(false);
                }
                else if(mouseActionType == MouseActionType.SelectPoints)
                {
                    if(!pointSelectionRectTransform.gameObject.activeSelf)
                        pointSelectionRectTransform.gameObject.SetActive(true);
                    SelectPoints(false);
                }
            }
            else if(touch.phase == TouchPhase.Ended && initialMouseInsideBounds)
            {
                if(mouseActionType == MouseActionType.SelectAreaToZoom)
                    SelectAreaToZoom(true);
                else if(mouseActionType == MouseActionType.SelectPoints)
                    SelectPoints(true);
                recentlyLockedPoints.Clear();
            }
        }
    }
    private void SelectAreaToZoom(bool release)
    {
        Vector2 relativeInitialMousePos = (new Vector2(initialMousePos.x, initialMousePos.y) - new Vector2(graphContent.transform.position.x, graphContent.transform.position.y)) / contentScale;

        Vector2 preserveAspectRatioCorner = new Vector2(mousePos.x, relativeInitialMousePos.y + (mousePos.x - relativeInitialMousePos.x) / GS.GraphSize.x * GS.GraphSize.y * contentScale.x / contentScale.y);
        Vector2 originalAspectRatioCorner = new Vector2(mousePos.x, relativeInitialMousePos.y + (mousePos.x - relativeInitialMousePos.x));
        
        Vector2 newCorner = rectangleType == RectangleType.Free ? mousePos : (rectangleType == RectangleType.PreserveAspectRatio ? preserveAspectRatioCorner : originalAspectRatioCorner);
        if(!release)
        {
            zoomSelectionRectTransform.anchoredPosition = (relativeInitialMousePos + (newCorner - relativeInitialMousePos) / 2f) * contentScale;
            Vector2 areaSize = new Vector2(Mathf.Abs(newCorner.x - relativeInitialMousePos.x), Mathf.Abs(newCorner.y - relativeInitialMousePos.y)) * contentScale;
            zoomSelectionRectTransform.sizeDelta = areaSize;
            zoomSelectionImage.color = GS.ZoomSelectionColor;
            for(int i = 0; i < zoomSelectionOutlines.Count; i++)
            {
                if (i % 2 == 0) // Left and Right outlines
                {
                    zoomSelectionOutlines[i].sizeDelta = new Vector2(GS.ZoomSelectionOutlineWidth, areaSize.y + GS.ZoomSelectionOutlineWidth * 2);
                    zoomSelectionOutlines[i].anchoredPosition = new Vector2((i == 0 ? -1 : 1) * (areaSize.x + GS.ZoomSelectionOutlineWidth) / 2, 0);
                }
                else // Top and Bottom outlines
                {
                    zoomSelectionOutlines[i].sizeDelta = new Vector2(areaSize.x + GS.ZoomSelectionOutlineWidth * 2, GS.ZoomSelectionOutlineWidth);
                    zoomSelectionOutlines[i].anchoredPosition = new Vector2(0, (i == 1 ? -1 : 1) * (areaSize.y + GS.ZoomSelectionOutlineWidth) / 2);
                }
                zoomSelectionOutlineImages[i].color = GS.ZoomSelectionOutlineColor;
            }
        }
        else
        {
            zoomSelectionRectTransform.gameObject.SetActive(false);
            Vector2 newBottomLeft = new Vector2(Mathf.Min(relativeInitialMousePos.x, newCorner.x), Mathf.Min(relativeInitialMousePos.y, newCorner.y));
            Vector2 newTopRight = new Vector2(Mathf.Max(relativeInitialMousePos.x, newCorner.x), Mathf.Max(relativeInitialMousePos.y, newCorner.y));
            if((newTopRight - newBottomLeft).magnitude > (topRight - bottomLeft).magnitude / 16f)
                SetCornerValues(newBottomLeft, newTopRight);
        }
        
    }
    private void SelectPoints(bool release)
    {
        Vector2 relativeInitialMousePos = (new Vector2(initialMousePos.x, initialMousePos.y) - new Vector2(graphContent.transform.position.x, graphContent.transform.position.y)) / contentScale;

        if (!release)
        {
            Vector2 newCorner = mousePos;

            pointSelectionRectTransform.anchoredPosition = (relativeInitialMousePos + (newCorner - relativeInitialMousePos) / 2f) * contentScale;
            Vector2 areaSize = new Vector2(Mathf.Abs(newCorner.x - relativeInitialMousePos.x), Mathf.Abs(newCorner.y - relativeInitialMousePos.y)) * contentScale;
            pointSelectionRectTransform.sizeDelta = areaSize;
            pointSelectionImage.color = GS.PointSelectionColor;

            for (int i = 0; i < pointSelectionOutlines.Count; i++)
            {
                if (i % 2 == 0) // Left and Right outlines
                {
                    pointSelectionOutlines[i].sizeDelta = new Vector2(GS.PointSelectionOutlineWidth, areaSize.y + GS.PointSelectionOutlineWidth * 2);
                    pointSelectionOutlines[i].anchoredPosition = new Vector2((i == 0 ? -1 : 1) * (areaSize.x + GS.PointSelectionOutlineWidth) / 2, 0);
                }
                else // Top and Bottom outlines
                {
                    pointSelectionOutlines[i].sizeDelta = new Vector2(areaSize.x + GS.PointSelectionOutlineWidth * 2, GS.PointSelectionOutlineWidth);
                    pointSelectionOutlines[i].anchoredPosition = new Vector2(0, (i == 1 ? -1 : 1) * (areaSize.y + GS.PointSelectionOutlineWidth) / 2);
                }
                pointSelectionOutlineImages[i].color = GS.PointSelectionOutlineColor;
            }
            if(rectangleSelectionPhase == RectangleSelectionPhase.Moving)
            {
                Vector2 newBottomLeft = new Vector2(Mathf.Min(relativeInitialMousePos.x, mousePos.x), Mathf.Min(relativeInitialMousePos.y, mousePos.y));
                Vector2 newTopRight = new Vector2(Mathf.Max(relativeInitialMousePos.x, mousePos.x), Mathf.Max(relativeInitialMousePos.y, mousePos.y));
                PointSelect(true, newBottomLeft, newTopRight);
            }
        }
        else
        {
            pointSelectionRectTransform.gameObject.SetActive(false);
            Vector2 newBottomLeft = new Vector2(Mathf.Min(relativeInitialMousePos.x, mousePos.x), Mathf.Min(relativeInitialMousePos.y, mousePos.y));
            Vector2 newTopRight = new Vector2(Mathf.Max(relativeInitialMousePos.x, mousePos.x), Mathf.Max(relativeInitialMousePos.y, mousePos.y));
            if(rectangleSelectionPhase == RectangleSelectionPhase.Release)
            {
                PointSelect(false, newBottomLeft, newTopRight);
            }
        }
    }
    private void PointSelect(bool moving, Vector2 newBottomLeft, Vector2 newTopRight)
    {
        for (int index = (int)xAxisRange.x; index <= (int)xAxisRange.y; index++)
        {
            Vector2 pointValue = values[index];
            if (pointValue.x >= newBottomLeft.x && pointValue.x <= newTopRight.x && pointValue.y >= newBottomLeft.y && pointValue.y <= newTopRight.y)
            {
                if(rectangleSelectionType == RectangleSelectionType.SelectUnselect)
                {
                    if(moving)
                    {
                        if (initialLockedPoints.Contains(index) && lockedPoints.Contains(index))
                        {
                            lockedPoints.Remove(index);
                            recentlyLockedPoints.Add(index);
                        }
                        else if(!initialLockedPoints.Contains(index) && !lockedPoints.Contains(index))
                        {
                            lockedPoints.Add(index);
                            recentlyLockedPoints.Add(index);
                        }
                            
                    }
                    else
                    {
                        if(lockedPoints.Contains(index))
                            lockedPoints.Remove(index);
                        else
                            lockedPoints.Add(index);
                    } 
                }
                else
                {
                    if(moving)
                    {
                        if(!initialLockedPoints.Contains(index) && !lockedPoints.Contains(index))
                        {
                            lockedPoints.Add(index);
                            recentlyLockedPoints.Add(index);
                        }    
                    }
                    else
                    {
                        if(!lockedPoints.Contains(index))
                            lockedPoints.Add(index);
                    }
                }
                lockedHoveredPoints.Add(index);
                lockedHoveredPoints = lockedHoveredPoints.Distinct().ToList();
                lockedPoints = lockedPoints.Distinct().ToList();
            }
        }
        for(int i = 0; i < recentlyLockedPoints.Count; i++)
        {
            int index = recentlyLockedPoints[i];
            Vector2 pointValue = values[index];
            
            if(!(pointValue.x >= newBottomLeft.x && pointValue.x <= newTopRight.x && pointValue.y >= newBottomLeft.y && pointValue.y <= newTopRight.y))
            {
                if(initialLockedPoints.Contains(index))
                {
                    if(!lockedPoints.Contains(index))
                        lockedPoints.Add(index);
                }
                else
                {
                    if(lockedPoints.Contains(index))
                        lockedPoints.Remove(index);
                }
            }
            lockedHoveredPoints.Add(index);
            lockedHoveredPoints = lockedHoveredPoints.Distinct().ToList();
            lockedPoints = lockedPoints.Distinct().ToList();
        }
    }
    private int MinMaxBinarySearch(bool findLeft) //important for large numbers of points
    {
        //this function finds the points that are closest to the sides of the graph window
        float target;
        target = findLeft ? bottomLeft.x : topRight.x;
        int min = 0;
        int max = sortedIndices.Count - 1;
        float value;
        while (min <= max)
        {
            int middle = min + (max - min) / 2;
            value = values[sortedIndices[middle]].x;
            if ((findLeft ? value >= target : value <= target))
            {
                if ((findLeft && (middle == 0 || values[sortedIndices[middle - 1]].x < target)) ||
                    (!findLeft && (middle == sortedIndices.Count - 1 || values[sortedIndices[middle + 1]].x > target)))
                {
                    return middle;
                }

                if (findLeft)
                    max = middle - 1;
                else
                    min = middle + 1;
            }
            else
            {
                if (findLeft)
                    min = middle + 1;
                else
                    max = middle - 1;
            }
        }
        return -1;
    }
    private void UpdateSizeDelta(RectTransform rect, Vector2 size)
    {
        if(Mathf.Abs(rect.sizeDelta.x - size.x) > 0.1f || Mathf.Abs(rect.sizeDelta.y - size.y) > 0.1f)
            rect.sizeDelta = size;
    }
    private void UpdateAnchoredPosition(RectTransform rect, Vector2 position)
    {
        if(Mathf.Abs(rect.sizeDelta.x - position.x) > 0.1f || Mathf.Abs(rect.sizeDelta.y - position.y) > 0.1f)
            rect.anchoredPosition = position;
    }
    [Flags]
    public enum UpdateMethod
    {
        UpdatePositionAndScale = 1 << 0,
        UpdateOutlines = 1 << 1,
        UpdatePointVisuals = 1 << 2,
        UpdateContent = 1 << 3,
        MouseZoom = 1 << 4,
        MouseAction = 1 << 5,
        UpdateGridLines = 1 << 6,
        All = 1 << 7
    }
    private bool CheckForErrors()
    {
        if(GetComponent<GraphSettings>() == null)
        {
            Debug.LogError("This GameObject has no GraphSettings script attached. Attach GraphSettings and restart");
            error = true;
            return true;
        }
        if(GetComponent<GraphSettings>().GridTextFont == null)
        {
            Debug.LogError("No font was found. Assign a font for GraphSettings.GridTextFont and restart");
            error = true;
        }
        if(GetComponent<GraphSettings>().PointSprite == null)
        {
            Debug.LogError("No point sprite was found. Assign a sprite for GraphSettings.PointSprite and restart");
            error = true;
        }
        if(error)
            return true;
        return false;
    }
    #endregion
}


