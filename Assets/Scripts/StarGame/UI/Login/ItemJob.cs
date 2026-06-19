using StarProject.Service.LocalData;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    public class ItemJob : MonoBehaviour
    {
        public JButton M_Btn;
        // 正常
        public GameObject M_NormalGob;
        public Image M_NormalJobIcon;           //角色icon
        public Text M_NormalJobName;            //昵称
        // 选中
        public GameObject M_SelectedGob;
        public Image M_SelectedJobIcon;           //角色icon
        public Text M_SelectedJobName;               //等级

        public CharacterCreateDataCell M_CharacterDataCell;  // 创角信息
        public JobDataCell M_JobDataCell;   // 职业信息
        public ModelDataCell M_ModelDataCell;  // 模型信息
        public AvatarDataCell M_AvatarDataCell;  // 模型信息

        public List<string> M_TransferJobName = new List<string>();


        public void InitItemJobInfo(CharacterCreateDataCell characterDataCell)
        {
            M_CharacterDataCell = characterDataCell;
            M_TransferJobName.Clear();
            if (M_CharacterDataCell == null)
            {
                gameObject.SetActive(false);
                M_ModelDataCell = null;
                M_AvatarDataCell = null;
                M_JobDataCell = null;
            }
            else
            {
                M_JobDataCell = LocalDataManager.Instance.GetJobDataCell(M_CharacterDataCell.GetJobID());
                if (M_JobDataCell != null)
                {
                    M_AvatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(M_JobDataCell.HighAvatarID);
                    if (M_AvatarDataCell != null)
                    {
                        M_ModelDataCell = LocalDataManager.Instance.GetModelDataCell(M_AvatarDataCell.GetHighModelId());
                    }
                    //M_NormalJobIcon
                    M_NormalJobName.text = M_JobDataCell.Name;
                    //M_SelectedRoleIcon
                    M_SelectedJobName.text = M_JobDataCell.Name;

                    // 可转职的职业名
                    var transferJobList = M_JobDataCell.TransferJob;
                    foreach (var item in transferJobList)
                    {
                        var jobCfg = LocalDataManager.Instance.GetJobDataCell(item);
                        if (jobCfg != null)
                        {
                            M_TransferJobName.Add(jobCfg.Name);
                        }
                    }
                }

                gameObject.SetActive(true);
            }
            SetSelected(false);
        }

        public void SetSelected(bool isSelect)
        {
            M_NormalGob.SetActive(!isSelect);
            M_SelectedGob.SetActive(isSelect);
        }
    }
}
