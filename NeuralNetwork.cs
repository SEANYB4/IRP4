
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class NeuralNetwork
{

    private int[] layers;
    private float[][] neurons;
    private float[][][] weights;
    private System.Random random;

    float learningRate = 0.01f;


    public NeuralNetwork(int[] layers)
    {
        this.layers = layers;
        random = new System.Random();
        InitializeNeurons();
        InitializeWeights();
        
    }


    private void InitializeNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }

        neurons = neuronsList.ToArray();
    }


    private void InitializeWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int neuronsInPreviousLayer = layers[i - 1];

            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    neuronWeights[k] = (float)random.NextDouble() - 0.5f;
                }
                layerWeightsList.Add(neuronWeights);

            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();
    }


    public float[] FeedForward(float[] inputs)
    {

        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        for (int i = 1; i < layers.Length; i++)
        {
            int layer = i - 1;
            for (int j = 0; j < neurons[i].Length; j++)
            {

                float value = 0f;
                for (int k = 0; k < neurons[layer].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[layer][k];
                }

                neurons[i][j] = (float)Math.Tanh(value); // Activation function       
            }
        }

        return neurons[neurons.Length - 1];


    }


    public void BackPropagate(float[] expected)
    {
        
        float[][] outputDeltas = new float[layers.Length][];

        // Calculate delta for output layer
        outputDeltas[layers.Length - 1] = new float[layers.Last()];
        for (int i = 0; i < layers.Last(); i++)
        {
            float error = expected[i] - neurons[layers.Length - 1][i];
            outputDeltas[layers.Length - 1][i] = error * TanhDerivative(neurons[layers.Length - 1][i]);

        }


        // Calculate delta for hidden layers
        for (int i = layers.Length - 2; i > 0; i--)
        {
            outputDeltas[i] = new float[layers[i]];
            for (int j = 0; j < layers[i]; j++)
            {
                float error = 0f;
                for (int k = 0; k < layers[i + 1]; k++)
                {

                    error += outputDeltas[i + 1][k] * weights[i][k][j];
                }
                outputDeltas[i][j] = error * TanhDerivative(neurons[i][j]);
            }
        }

        // Update weights
        
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    weights[i - 1][j][k] += learningRate * outputDeltas[i][j] * neurons[i - 1][k];
                    
                }
                
            }
        }
    }


    // Derivative of Tanh
    private float TanhDerivative(float value)
    {
        return 1 - (value * value);
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Method to save weights
    public void SaveWeights(string filePath)
    {
        string json = JsonUtility.ToJson(new WeightsContainer { Weights = FlattenWeights()});
        File.WriteAllText(filePath, json);
        UnityEngine.Debug.Log("Weights saved to " + filePath);
    }


    // Method to load weights

    public void LoadWeights(string filePath)
    {

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            WeightsContainer container = JsonUtility.FromJson<WeightsContainer>(json);
            UnflattenWeights(container.Weights);
            UnityEngine.Debug.Log("Weights loaded from " + filePath);
        }
    }


    // Converts 3D weight array to 1D list for easier serialization
    private float[] FlattenWeights()
    {

        List<float> flatWeights = new List<float>();
        foreach (var matrix in weights)
        {
            foreach ( var row in matrix)
            {
                flatWeights.AddRange(row);
            }
        }

        return flatWeights.ToArray();
    }


    // Converts 1D list back to 3D weight array

    private void UnflattenWeights(float[] flatWeights)
    {
        int index = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = flatWeights[index++];
                }
            }
        }
    }


    [Serializable]
    private class WeightsContainer
    {
        public float[] Weights;
        
    }

}
