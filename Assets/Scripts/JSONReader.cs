
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;


public class JSONReader
{
    private string folderPath;
    private int curIndex; // Index should be controlled only from the class methods
    string startPoseFile;
    string targetPoseFile;
    string stopoverPoseFile;
    string[] fileNames;
    public int numFiles;
    
    public JSONReader(string dataPath)
    {
        folderPath = dataPath;
        Debug.Log("Data path is set to : " + folderPath.ToString());
        startPoseFile = Path.Combine(folderPath, "start.json");
        targetPoseFile = Path.Combine(folderPath, "target.json");
        stopoverPoseFile = Path.Combine(folderPath, "stopover.json");
        fileNames = Directory.GetFiles(folderPath, "*.json").Where(file => Regex.IsMatch(Path.GetFileName(file), "^[0-9]+")).ToArray();
        fileNames = fileNames.OrderBy(d => d).ToArray(); // make sure files are soreted in ascending order
        numFiles = fileNames.Length;
        Debug.Log("Num files: " + numFiles.ToString());
        Debug.Log("Files: " + fileNames);
        curIndex = 0;
    }

    MotionData ParseJSON(string fileName)
    {
        StreamReader reader = new StreamReader(fileName);
        Debug.Log(fileName);
        string content = reader.ReadToEnd();
        reader.Close();
        MotionData parsed = JsonConvert.DeserializeObject<MotionData>(content);
        return parsed;
    }

    public MotionData GetStartData()
    {
        MotionData startData = ParseJSON(startPoseFile);
        return startData;
    }

    public MotionData GetTargetData()
    {
        MotionData targetData = ParseJSON(targetPoseFile);
        return targetData;
    }

    public MotionData GetStopoverData()
    {
        MotionData stopoverData = ParseJSON(stopoverPoseFile);
        return stopoverData;
    }

    public MotionData GetMotionData()
    {
        MotionData jsonData = ParseJSON(fileNames[curIndex]);
        curIndex++;
        return jsonData;
    }

    public MotionData GetMotionData(int timestep)
    {
        MotionData jsonData = ParseJSON(fileNames[timestep]);
        return jsonData;
    }

    public int GetCurIndex()
    {
        return curIndex;
    }
}
