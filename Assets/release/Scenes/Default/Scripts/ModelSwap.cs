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

public class ModelSwap : MonoBehaviour
{
    // Models

    public NNModel m_InitialModel;
    // public Agent agent;

    [HideInInspector] public List<NNModel> nnModelList;
    [HideInInspector] public int currentModel = 0;
    [HideInInspector] public int m_PastModel = 0;
    [HideInInspector] public int m_GetupModel = 4;

    // Path to .onnx files
    string m_relPath = string.Empty;

    private void Start()
    {
        // Load Models From Dir
        NNFileList();
    }


    public void NNFileList()
    {
        m_relPath = Application.dataPath;
        m_relPath = Path.GetDirectoryName(m_relPath);
        m_relPath = m_relPath + "\\ModelSwaper\\ModelList";
        DirectoryInfo dirInfo = new DirectoryInfo(m_relPath);
        FileInfo[] nnList = dirInfo.GetFiles("*.onnx");

        // Sort files by creation date
        Array.Sort(nnList, delegate (FileInfo x, FileInfo y) { return DateTime.Compare(x.CreationTime, y.CreationTime); });

        ConvertNNModels(nnList);
    }

    public void ConvertNNModels(FileInfo[] nnList)
    {
        int i = 0;
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

            if(result.name == "Getup.onnx")
            {
                m_GetupModel = i;
            }

            i++;
        }

        Debug.Log("Total Number of Models: " + nnModelList.Count);
        // Set the inital model to 0
        m_InitialModel = nnModelList[currentModel];
        GlobalVars.g_CurrentModel = nnModelList[currentModel].name;
    }

    public void SwitchModel(int modelActive, Agent inst)
    {
        // If left, move down in array
        if (modelActive == 0)
        {
            currentModel -= 1;
            if(currentModel < 0)
            {
                currentModel = 0;
            }

            inst.SetModel("Swap", nnModelList[currentModel]);
            GlobalVars.g_CurrentModel = nnModelList[currentModel].name;
        }
        // Set original Model
        else if (modelActive == 3)
        {
            inst.SetModel("Swap", m_InitialModel);
        }
        // If right, move up in array
        else if (modelActive == 1) 
        {
            currentModel += 1;
            if (currentModel > nnModelList.Count - 1)
            {
                currentModel = nnModelList.Count - 1;
            }

            inst.SetModel("Swap", nnModelList[currentModel]);
            GlobalVars.g_CurrentModel = nnModelList[currentModel].name;
        }
        // Swap model if falls over, and store past model number
        else if (modelActive == 4) 
        {
            m_PastModel = currentModel;
            currentModel = m_GetupModel;
            inst.SetModel("Swap", nnModelList[currentModel]);
            GlobalVars.g_CurrentModel = nnModelList[currentModel].name;
        }
        // Swap model back to past when it stabalises
        else if (modelActive == 5)
        {
            currentModel = m_PastModel;
            inst.SetModel("Swap", nnModelList[currentModel]);
            GlobalVars.g_CurrentModel = nnModelList[currentModel].name;
        }

        Debug.Log("Current Model: " + GlobalVars.g_CurrentModel);
    }
};
