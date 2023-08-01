using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using Unity.MLAgents.Policies;
using System;
using Unity.Barracuda.ONNX;
using System.IO;
using System.Linq;

public class SwapModel : MonoBehaviour
{
    // Models
    public NNModel green;
    public NNModel red;
    public Agent agent;

    [HideInInspector] public List<NNModel> nnModelList;
    [HideInInspector] public int currentModel = 0;

    // Path to .onnx files
    string dirPath = "C:\\Users\\caile\\Desktop\\Projects\\23_07-MLAgents\\results\\23_08_01-07\\Swap";

    private void Start()
    {

        // Load Models From Dir
        NNFileList();
    }


    public void NNFileList()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
        FileInfo[] nnList = dirInfo.GetFiles("*.onnx");

        // Sort files by creation date
        Array.Sort(nnList, delegate (FileInfo x, FileInfo y) { return DateTime.Compare(x.CreationTime, y.CreationTime); });

        ConvertNNModels(nnList);
    }

    public void ConvertNNModels(FileInfo[] nnList)
    {
        foreach (FileInfo element in nnList) 
        {
            var converter = new ONNXModelConverter(true);
            byte[] modelData = File.ReadAllBytes(element.FullName.ToString());
            Model model = converter.Convert(modelData);
            NNModelData modelD = ScriptableObject.CreateInstance<NNModelData>();
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                ModelWriter.Save(writer, model);
                modelD.Value = memoryStream.ToArray();
            }
            modelD.name = "Data";
            modelD.hideFlags = HideFlags.HideInHierarchy;
            NNModel result = ScriptableObject.CreateInstance<NNModel>();
            result.modelData = modelD;
            result.name = element.Name;
            // Add Model to Model List
            nnModelList.Add(result);
        }

        // Set the inital model to 0
        red = nnModelList[currentModel];
    }

    public void SwitchModel(int modelActive)
    {
        // If left, move down in array
        if (modelActive == 0)
        {
            currentModel -= 1;
            if(currentModel < 0)
            {
                currentModel = 0;
            }

            agent.SetModel("Swap", nnModelList[currentModel]);
        }
        else if (modelActive == 3)
        {
            agent.SetModel("Swap", green);
        }
        // If right, move up in array
        else
        {
            currentModel += 1;
            if (currentModel > nnModelList.Count - 1)
            {
                currentModel = nnModelList.Count - 1;
            }

            agent.SetModel("Swap", nnModelList[currentModel]);
        }
    }
};
