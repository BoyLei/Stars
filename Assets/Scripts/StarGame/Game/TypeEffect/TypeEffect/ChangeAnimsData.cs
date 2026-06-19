using System.Collections.Generic;

public class ChangeAnimsData
{
    public string Tag;
    public List<SkillEditor.ChangeAnim> ChangeAnims;

    public void Init(string tag, List<SkillEditor.ChangeAnim> changeAnims)
    {
        Tag = tag;
        ChangeAnims = changeAnims;
    }

    public string GetAnim(SkillEditor.AnimState animState)
    {
        SkillEditor.ChangeAnim findResult = ChangeAnims.Find((SkillEditor.ChangeAnim changeAnim) =>
        {
            return changeAnim.AnimState == animState;
        });
        if (findResult == null)
        {
            return null;
        }
        return findResult.Anim;
    }
}