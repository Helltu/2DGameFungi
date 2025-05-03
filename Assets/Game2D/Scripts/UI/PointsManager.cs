using System;
using System.Collections.Generic;
using System.Drawing;
using Game2D.Scripts.Collectables;
using TMPro;
using UnityEngine;

namespace Game2D.Scripts.UI
{
    public class PointsManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text pointsText;
        private PointGiver[] _pointGivers;
        private int _currentPoints;

        private void Start()
        {
            _pointGivers = FindObjectsByType<PointGiver>(FindObjectsSortMode.None);
            pointsText.text = _currentPoints.ToString();

            OnEnable();
        }

        private void OnEnable()
        {
            if(_pointGivers == null)
                return;
            
            foreach (var pointGiver in _pointGivers)
            {
                pointGiver.OnPointCollected += PointGiverOnOnPointCollected;
            }
        }

        private void OnDisable()
        {
            if(_pointGivers == null)
                return;
            
            foreach (var pointGiver in _pointGivers)
            {
                pointGiver.OnPointCollected -= PointGiverOnOnPointCollected;
            }
        }

        private void PointGiverOnOnPointCollected(object sender, EventArgs e)
        {
            pointsText.text = (++_currentPoints).ToString();
        }
    }
}