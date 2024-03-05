using UnityEditor;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class MixerEnumGenerator
{
    // Auxiliary Class to generate Mixer Content file
    private class MixerContent
    {
        public MixerContent(AudioMixer _mixer, AudioMixerGroup[] _mixerGroups, string[] _mixerExposedParameters)
        {
            mixer = _mixer;
            mixerGroups = _mixerGroups;
            mixerExposedParameters = _mixerExposedParameters;
        }

        public AudioMixer mixer;
        public AudioMixerGroup[] mixerGroups;
        public string[] mixerExposedParameters;
    }


    [MenuItem("MixerManager/Create Mixer Content Files")]
    private static void CreateMixerContentFiles()
    {
        List<MixerContent> MixersInProject = new List<MixerContent>();

        FillMixerList(MixersInProject);
        CreateMixerEnumFile(MixersInProject);
        AssetDatabase.Refresh();
    }

    private static void FillMixerList(List<MixerContent> mixerListToFill)
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioMixer", null);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioMixer asset = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);

            if (asset != null)
            {
                List<string> exposedParameters = new List<string>();
                System.Array para = (System.Array)asset.GetType().GetProperty("exposedParameters").GetValue(asset, null);
                for (int i = 0; i < para.Length; i++)
                {
                    System.Object extractedParam = para.GetValue(i);
                    string convertedParam = (string)extractedParam.GetType().GetField("name").GetValue(extractedParam);
                    exposedParameters.Add(convertedParam);
                }

                mixerListToFill.Add(new MixerContent(asset, asset.FindMatchingGroups(string.Empty), exposedParameters.ToArray()));
            }
        }
    }

    private static void CreateMixerEnumFile(List<MixerContent> filledMixerList)
    {
        string assetNewFile = $"Assets/AudioManager/Scripts/AudioManager/MixersEnums.cs";
        Regex regex = new Regex(@"[@$%&\/:*?'<>|~`#^+={}[;!\] ]", RegexOptions.IgnoreCase);

        if (File.Exists(assetNewFile))
        {
            File.Delete(assetNewFile);
        }

        List<AudioMixer> Mixers = new List<AudioMixer>();
        foreach (MixerContent mixerItem in filledMixerList)
        {
            Mixers.Add(mixerItem.mixer);
        }

        using (StreamWriter newFile = new StreamWriter(assetNewFile))
        {
            newFile.WriteLine($"public enum MixerType_Test");
            newFile.WriteLine("{");
            for (int i = 0; i < Mixers.Count; i++)
            {
                string mixerLine = $"\t{regex.Replace(Mixers[i].name, string.Empty)},";
                if (i + 1 >= Mixers.Count) mixerLine = mixerLine.Replace(",", "");

                newFile.WriteLine(mixerLine);
            }
            newFile.WriteLine("}");
            newFile.WriteLine("");

            newFile.WriteLine($"public enum MixersGroupsType_Test");
            newFile.WriteLine("{");
            foreach (MixerContent mixerItem in filledMixerList)
            {
                for (int j = 0; j < mixerItem.mixerGroups.Length; j++)
                {
                    string mixerGroupLine = $"\t{regex.Replace(mixerItem.mixer.name, string.Empty)}{regex.Replace(mixerItem.mixerGroups[j].name, string.Empty)},";
                    if (j + 1 >= mixerItem.mixerGroups.Length) mixerGroupLine = mixerGroupLine.Replace(",", "");

                    newFile.WriteLine(mixerGroupLine);
                }
                newFile.WriteLine("}");
                newFile.WriteLine("");

                newFile.WriteLine($"public enum MixersExposedParametersType_Test");
                newFile.WriteLine("{");
                for (int k = 0; k < mixerItem.mixerExposedParameters.Length; k++)
                {
                    string mixerExposedParametersLine = $"\t{regex.Replace(mixerItem.mixer.name, string.Empty)}{mixerItem.mixerExposedParameters[k]},";
                    if (k + 1 >= mixerItem.mixerExposedParameters.Length) mixerExposedParametersLine = mixerExposedParametersLine.Replace(",", "");

                    newFile.WriteLine(mixerExposedParametersLine);
                }
                newFile.WriteLine("}");
            }
        }
    }
}