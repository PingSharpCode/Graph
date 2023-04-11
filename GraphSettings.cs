using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphSettings : MonoBehaviour
{
    [Header("Graph Settings")]
    [Space]
    public int updatePeriod = 5;
    [SerializeField] private Vector2 graphSize = new Vector2(800f, 400f);
    public Vector2 GraphSize
    {
        get { return graphSize; }
        set { graphSize = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePositionAndScale | GraphHandler.UpdateMethod.UpdateContent | GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [SerializeField] private Vector2 graphScale = new Vector2(100f, 100f);
    public Vector2 GraphScale
    {
        get { return graphScale; }
        set { graphScale = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePositionAndScale | GraphHandler.UpdateMethod.UpdateContent | GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [Space]
    [Header("Graph Visuals")]
    [Space]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 1f);
    public Color BackgroundColor
    {
        get { return backgroundColor; }
        set { backgroundColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateOutlines); }
    }
    [SerializeField] private float outlineWidth = 5f;
    public float OutlineWidth
    {
        get { return outlineWidth; }
        set { outlineWidth = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateOutlines); }
    }

    [SerializeField] private Color outlineColor = new Color(0, 0.8f, 1f, 1f);
    public Color OutlineColor
    {
        get { return outlineColor; }
        set { outlineColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateOutlines); }
    }
    [Space]
    [SerializeField] private float lineWidth = 8f;
    public float LineWidth
    {
        get { return lineWidth; }
        set { lineWidth = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateContent); }
    }
    [SerializeField] private Color lineColor = new Color(1f, 0.35f, 0f, 1f);
    public Color LineColor
    {
        get { return lineColor; }
        set { lineColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateContent); }
    }
    [Space]
    public Sprite PointSprite;
    [SerializeField] private float pointRadius = 5f;
    public float PointRadius
    {
        get { return pointRadius; }
        set { pointRadius = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    [SerializeField] private Color pointColor = new Color(1f, 0.35f, 0f, 1f);
    public Color PointColor
    {
        get { return pointColor; }
        set { pointColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    [Space]
    [SerializeField] private float pointHoverRadius = 15f;
    public float PointHoverRadius
    {
        get { return pointHoverRadius; }
        set { pointHoverRadius = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    public float PointHoverSpeed = 5f;

    [SerializeField] private Color pointHoverColor = new Color(1, 0.6f, 0, 1f);
    public Color PointHoverColor
    {
        get { return pointHoverColor; }
        set { pointHoverColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    [Space]
    [SerializeField] private float pointLockedRadius = 17f;
    public float PointLockedRadius
    {
        get { return pointLockedRadius; }
        set { pointLockedRadius = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    public float PointLockedSpeed = 5f;

    [SerializeField] private Color pointLockedColor = new Color(1, 0.8f, 0, 1f);
    public Color PointLockedColor
    {
        get { return pointLockedColor; }
        set { pointLockedColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    [Space]
    [SerializeField] private float unfixedPointOutlineWidth = 10f;
    public float UnfixedPointOutlineWidth
    {
        get { return unfixedPointOutlineWidth; }
        set { unfixedPointOutlineWidth = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    [SerializeField] private Color unfixedPointOutlineColor = new Color(0, 0.8f, 1f, 1f);
    public Color UnfixedPointOutlineColor
    {
        get { return unfixedPointOutlineColor; }
        set { unfixedPointOutlineColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    [Space]
    [SerializeField] private float unfixedPointOutlineHoverWidth = 15f;
    public float UnfixedPointOutlineHoverWidth
    {
        get { return unfixedPointOutlineHoverWidth; }
        set { unfixedPointOutlineHoverWidth = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    public float UnfixedPointOutlineHoverSpeed = 5f;

    [Space]
    [SerializeField] private Color unfixedPointOutlineHoverColor = new Color(0, 0.5f, 1f, 1f);
    public Color UnfixedPointOutlineHoverColor
    {
        get { return unfixedPointOutlineHoverColor; }
        set { unfixedPointOutlineHoverColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    [Space]
    [SerializeField] private float fixedPointOutlineWidth = 17f;
    public float FixedPointOutlineWidth
    {
        get { return fixedPointOutlineWidth; }
        set { fixedPointOutlineWidth = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    public float FixedPointOutlineSpeed = 5f;
    [SerializeField] private Color fixedPointOutlineColor = new Color(0, 0.8f, 1f, 1f);
    public Color FixedPointOutlineColor
    {
        get { return fixedPointOutlineColor; }
        set { fixedPointOutlineColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdatePointVisuals); }
    }
    
    [Space]
    [Header("Grid Settings")]
    [Space]

    public TMP_FontAsset GridTextFont;
    [SerializeField] private Vector2 gridSpacing = new Vector2(1, 1);
    public Vector2 GridSpacing
    {
        get { return gridSpacing; }
        set { gridSpacing = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [Space]
    [SerializeField] private float xAxisWidth = 3f;
    public float XAxisWidth
    {
        get { return xAxisWidth; }
        set { xAxisWidth = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }

    [SerializeField] private Color xAxisColor = new Color(0, 0.8f, 1f, 1f);
    public Color XAxisColor
    {
        get { return xAxisColor; }
        set { xAxisColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [Space]
    [SerializeField] private Color xAxisTextColor = new Color(0, 0.8f, 1f, 1f);
    public Color XAxisTextColor
    {
        get { return xAxisTextColor; }
        set { xAxisTextColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [SerializeField] private float xAxisTextSize = 10f;
    public float XAxisTextSize
    {
        get { return xAxisTextSize; }
        set { xAxisTextSize = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [SerializeField] private float xAxisTextOffset = 10f;
    public float XAxisTextOffset
    {
        get { return xAxisTextOffset; }
        set { xAxisTextOffset = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [Space]
    [SerializeField] private float yAxisWidth = 3f;
    public float YAxisWidth
    {
        get { return yAxisWidth; }
        set { yAxisWidth = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }

    [SerializeField] private Color yAxisColor = new Color(0, 0.8f, 1f, 1f);
    public Color YAxisColor
    {
        get { return yAxisColor; }
        set { yAxisColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [Space]
    [SerializeField] private Color yAxisTextColor = new Color(0, 0.8f, 1f, 1f);
    public Color YAxisTextColor
    {
        get { return yAxisTextColor; }
        set { yAxisTextColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [SerializeField] private float yAxisTextSize = 10f;
    public float YAxisTextSize
    {
        get { return yAxisTextSize; }
        set { yAxisTextSize = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [SerializeField] private float yAxisTextOffset = 10f;
    public float YAxisTextOffset
    {
        get { return yAxisTextOffset; }
        set { yAxisTextOffset = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [Space]
    [SerializeField] private float xGridWidth = 2f;
    public float XGridWidth
    {
        get { return xGridWidth; }
        set { xGridWidth = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [SerializeField] private Color xGridColor = new Color(0, 0.8f, 1f, 0.6f);
    public Color XGridColor
    {
        get { return xGridColor; }
        set { xGridColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    [Space]
    [SerializeField] private float yGridWidth = 2f;
    public float YGridWidth
    {
        get { return yGridWidth; }
        set { yGridWidth = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }

    [SerializeField] private Color yGridColor = new Color(0, 0.8f, 1f, 0.6f);
    public Color YGridColor
    {
        get { return yGridColor; }
        set { yGridColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.UpdateGridLines); }
    }
    
    [Space]
    [SerializeField] private Color zoomSelectionColor = new Color(0, 0.8f, 1f, 0.2f);
    public Color ZoomSelectionColor
    {
        get { return zoomSelectionColor; }
        set { zoomSelectionColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.MouseAction); }
    }
    [SerializeField] private float zoomSelectionOutlineWidth = 5f;
    public float ZoomSelectionOutlineWidth
    {
        get { return zoomSelectionOutlineWidth; }
        set { zoomSelectionOutlineWidth = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.MouseAction); }
    }
    [SerializeField] private Color zoomSelectionOutlineColor = new Color(0, 0.8f, 1f, 0.6f);
    public Color ZoomSelectionOutlineColor
    {
        get { return zoomSelectionOutlineColor; }
        set { zoomSelectionOutlineColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.MouseAction); }
    }
    [Space]
    [SerializeField] private Color pointSelectionColor = new Color(1, 0.35f, 0f, 0.2f);
    public Color PointSelectionColor
    {
        get { return pointSelectionColor; }
        set { pointSelectionColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.MouseAction); }
    }
    [SerializeField] private float pointSelectionOutlineWidth = 5f;
    public float PointSelectionOutlineWidth
    {
        get { return pointSelectionOutlineWidth; }
        set { pointSelectionOutlineWidth = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.MouseAction); }
    }
    [SerializeField] private Color pointSelectionOutlineColor = new Color(1, 0.35f, 0f, 0.4f);
    public Color PointSelectionOutlineColor
    {
        get { return pointSelectionOutlineColor; }
        set { pointSelectionOutlineColor = value; GH.UpdateGraphInternal(GraphHandler.UpdateMethod.MouseAction); }
    }
    [Space]
    public float ZoomSpeed = 5f;
    public float SmoothZoomSpeed = 20f;
    public float SmoothMoveSpeed = 20f;

    private GraphHandler GH;
    private void Awake()
    {
        GH = GetComponent<GraphHandler>();
    }
}
