using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionController : MonoBehaviour
{
    public string dataPath;
    public int translationScaling = 100;
    public float frameInterval = 0.05f;
    JSONReader dataReader;
    Vector3 initPos;
    GameObject referenceModel;

    GameObject rigModel;
    GameObject startModel;
    GameObject targetModel;
    GameObject stopoverModel;

    public bool stopover;

    RigStructure baseRig = new RigStructure();
    RigStructure startRig = new RigStructure();
    RigStructure targetRig = new RigStructure();
    RigStructure stopoverRig = new RigStructure();

    public Color startColor = Color.red;
    public Color targetColor = Color.blue;
    public Color stopoverColor = Color.green;
    public Color infillingColor = Color.yellow;

    // Start is called before the first frame update
    void Start()
    {
        
        GameObject referenceModel = transform.Find("model_skeleton").gameObject;

        dataReader = new JSONReader(dataPath);
        initPos = gameObject.transform.position;

        rigModel = Instantiate(referenceModel);
        rigModel.name += "_base";
        rigModel.GetComponentInChildren<SkinnedMeshRenderer>().materials[3].SetColor("_Color", infillingColor);

        startModel = Instantiate(referenceModel);
        startModel.name += "_start";
        startModel.GetComponentInChildren<SkinnedMeshRenderer>().materials[3].SetColor("_Color", startColor);

        targetModel = Instantiate(referenceModel);
        targetModel.name += "_target";
        targetModel.GetComponentInChildren<SkinnedMeshRenderer>().materials[3].SetColor("_Color", targetColor);

        baseRig = LinkRigStructure(rigModel, baseRig);
        startRig = LinkRigStructure(startModel, startRig);
        targetRig = LinkRigStructure(targetModel, targetRig);

        if (stopover){
            stopoverModel = Instantiate(referenceModel);
            stopoverModel.name += "_stopover";
            stopoverRig = LinkRigStructure(stopoverModel, stopoverRig);
            stopoverModel.GetComponentInChildren<SkinnedMeshRenderer>().materials[3].SetColor("_Color", stopoverColor);
        }

        referenceModel.SetActive(false);
        
        SetStartTargetPose();
        StartCoroutine(RunMotion());
    }

    IEnumerator RunMotion()
    {
        // yield return new WaitForSeconds(1.0f);
        while(dataReader.GetCurIndex() < dataReader.numFiles)
        {
            yield return new WaitForSeconds(frameInterval);
            MotionData motionData = dataReader.GetMotionData();
            SetPose(motionData, baseRig);
        }
    }

    Quaternion TransformJointRot(float[] joint)
    {
        //https://github.com/facebookresearch/QuaterNet/issues/6
        // Input representaion of quaternion: wxyz
        Quaternion quat = new Quaternion(joint[1], joint[2], joint[3], joint[0]);
        return quat;
    }

    void SetStartTargetPose()
    {
        MotionData startData = dataReader.GetStartData();
        SetPose(startData, startRig);
        startModel.GetComponentInChildren<Renderer>().material.SetColor("_Color", new Color(0.97f, 0.33f, 0.43f, 1.0f));

        MotionData targetData = dataReader.GetTargetData();
        SetPose(targetData, targetRig);
        targetModel.GetComponentInChildren<Renderer>().material.SetColor("_Color", new Color(0.3f, 0.5f, 1.0f, 1.0f));

        if (stopover){
            MotionData stopoverData = dataReader.GetStopoverData();
            SetPose(stopoverData, stopoverRig);
            stopoverModel.GetComponentInChildren<Renderer>().material.SetColor("_Color", new Color(0.97f, 0.33f, 0.43f, 1.0f));
        }
    }

    void SetPose(MotionData motionData, RigStructure rig)
    {
        float[] rootData = motionData.root_pos;
        Vector3 root_pos = new Vector3(rootData[0]/translationScaling, rootData[1]/translationScaling, rootData[2]/translationScaling);
        rig.Hips.transform.position = root_pos + initPos;

        float[][] quatData = motionData.local_quat;
        rig.Hips.transform.localRotation = TransformJointRot(quatData[0]);
        rig.LeftUpLeg.transform.localRotation = TransformJointRot(quatData[1]);
        rig.LeftLeg.transform.localRotation = TransformJointRot(quatData[2]);
        rig.LeftFoot.transform.localRotation = TransformJointRot(quatData[3]);
        // rig.LeftToe.transform.localRotation = TransformJointRot(quatData[4]);
        rig.RightUpLeg.transform.localRotation = TransformJointRot(quatData[5]);
        rig.RightLeg.transform.localRotation = TransformJointRot(quatData[6]);
        rig.RightFoot.transform.localRotation = TransformJointRot(quatData[7]);
        // rig.RightToe.transform.localRotation = TransformJointRot(quatData[8]);
        rig.Spine.transform.localRotation = TransformJointRot(quatData[9]);
        rig.Spine1.transform.localRotation = TransformJointRot(quatData[10]);
        rig.Spine2.transform.localRotation = TransformJointRot(quatData[11]);
        rig.Neck.transform.localRotation = TransformJointRot(quatData[12]);
        // rig.Head.transform.localRotation = TransformJointRot(quatData[13]);
        rig.LeftShoulder.transform.localRotation = TransformJointRot(quatData[14]);
        rig.LeftArm.transform.localRotation = TransformJointRot(quatData[15]);
        rig.LeftForeArm.transform.localRotation = TransformJointRot(quatData[16]);
        // rig.LeftHand.transform.localRotation = TransformJointRot(quatData[17]);
        rig.RightShoulder.transform.localRotation = TransformJointRot(quatData[18]);
        rig.RightArm.transform.localRotation = TransformJointRot(quatData[19]);
        rig.RightForeArm.transform.localRotation = TransformJointRot(quatData[20]);
        // rig.RightHand.transform.localRotation = TransformJointRot(quatData[21]);
    }

    RigStructure LinkRigStructure(GameObject model, RigStructure rig)
    {
        rig.Hips = model.transform.Find("Model:Hips").gameObject;
        rig.RightUpLeg = model.transform.Find("Model:Hips/Model:LeftUpLeg").gameObject;
        rig.RightLeg = model.transform.Find("Model:Hips/Model:LeftUpLeg/Model:LeftLeg").gameObject;
        rig.RightFoot = model.transform.Find("Model:Hips/Model:LeftUpLeg/Model:LeftLeg/Model:LeftFoot").gameObject;
        // rig.RightToe = model.transform.Find("Model:Hips/Model:LeftUpLeg/Model:LeftLeg/Model:LeftFoot/Model:LeftToe").gameObject;
        rig.LeftUpLeg = model.transform.Find("Model:Hips/Model:RightUpLeg").gameObject;
        rig.LeftLeg = model.transform.Find("Model:Hips/Model:RightUpLeg/Model:RightLeg").gameObject;
        rig.LeftFoot = model.transform.Find("Model:Hips/Model:RightUpLeg/Model:RightLeg/Model:RightFoot").gameObject;
        // rig.LeftToe = model.transform.Find("Model:Hips/Model:RightUpLeg/Model:RightLeg/Model:RightFoot/Model:RightToe").gameObject;
        rig.Spine = model.transform.Find("Model:Hips/Model:Spine").gameObject;
        rig.Spine1 = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1").gameObject;
        rig.Spine2 = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1/Model:Spine2").gameObject;
        rig.Neck = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1/Model:Spine2/Model:Neck").gameObject;
        // rig.Head = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1/Model:Spine2/Model:Neck/Model:Head").gameObject;
        rig.RightShoulder = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1/Model:Spine2/Model:LeftShoulder").gameObject;
        rig.RightArm = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1/Model:Spine2/Model:LeftShoulder/Model:LeftArm").gameObject;
        rig.RightForeArm = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1/Model:Spine2/Model:LeftShoulder/Model:LeftArm/Model:LeftForeArm").gameObject;
        // rig.RightHand = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1/Model:Spine2/Model:LeftShoulder/Model:LeftArm/Model:LeftForeArm/Model:LeftHand").gameObject;
        rig.LeftShoulder = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1/Model:Spine2/Model:RightShoulder").gameObject;
        rig.LeftArm = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1/Model:Spine2/Model:RightShoulder/Model:RightArm").gameObject;
        rig.LeftForeArm = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1/Model:Spine2/Model:RightShoulder/Model:RightArm/Model:RightForeArm").gameObject;
        // rig.LeftHand = model.transform.Find("Model:Hips/Model:Spine/Model:Spine1/Model:Spine2/Model:RightShoulder/Model:RightArm/Model:RightForeArm/Model:RightHand").gameObject;

        return rig;
    }

}
