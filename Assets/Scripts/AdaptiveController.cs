using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdaptiveController : MonoBehaviour
{

    [SerializeField] private float errorThresh;
    [SerializeField] private float scoreThresh;

    private const int windowSize = 1000;

    private float[] _scoreDiffArray = new float[windowSize];
    private float[] _errorArray = new float[windowSize];
    private float _filteredScoreDiff = 0;
    private float _filteredError = 0;
    private float _prevScore = 0;
    private int _iteration = 0;
    private bool started;



    public void addNewData(float newScore, float newError)
    {
        adaptionStep(newScore, newError);
    }

    private void adaptionStep(float newScore, float newError)
    {
        addNewElement(_scoreDiffArray, newScore - _prevScore);
        _prevScore = newScore;
        _filteredScoreDiff = _scoreDiffArray.Sum() / windowSize;

        addNewElement(_errorArray, newError);
        _filteredError = _errorArray.Sum() / windowSize;

        _iteration++;

        if(_iteration > windowSize)
        {
            //begin adapting
            if (checkThresholds()) adaptGame();
        }

    }

    private void addNewElement(float[] array, float newValue)
    {
        float[] temp = array;
        Array.Resize(ref temp, array.Length + 1);

        temp[array.Length] = newValue;

        Array.Copy(temp, 1, array, 0, windowSize);

    }

    private bool checkThresholds()
    {
        return (_filteredScoreDiff > scoreThresh || _filteredError > errorThresh) ;
    }

    private void adaptGame()
    {
        Debug.Log("Adapting game...");
        return;
    }

}
