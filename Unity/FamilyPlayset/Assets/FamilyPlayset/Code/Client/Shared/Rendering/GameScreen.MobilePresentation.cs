using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private bool presentationBuilding;
        private Coroutine presentationBuilder;
        private readonly HashSet<string> preparedRecoveredWorlds=new HashSet<string>();
        private readonly List<BuildStep> presentationSteps=new List<BuildStep>();
        [Serializable] private sealed class BuildStep { public string step;public double milliseconds; }
        [Serializable] private sealed class BuildEvidence { public string build,world;public bool shared;public BuildStep[] steps; }
        private void CancelPresentationBuild()
        {
            if(presentationBuilder!=null)StopCoroutine(presentationBuilder);
            presentationBuilder=null;presentationBuilding=false;preparedRecoveredWorlds.Clear();
        }
        private void StartMobilePresentation()
        {
            presentationBuilding=true;presentationSteps.Clear();
            presentationBuilder=StartCoroutine(BuildMobilePresentation());
        }
        private void BuildMeasured(Action action)
        {
            var timer=System.Diagnostics.Stopwatch.StartNew();action();timer.Stop();
            presentationSteps.Add(new BuildStep{step=action.Method.Name,milliseconds=timer.Elapsed.TotalMilliseconds});
        }
        private IEnumerator BuildMobilePresentation()
        {
            // Keep transport updates running between bounded UI construction
            // steps. A single multi-second join frame can lose the authority.
            BuildMeasured(BuildNavigation);
            loadingCaption.text="Getting your game ready…";loadingScreen.gameObject.SetActive(true);loadingScreen.SetAsLastSibling();
            yield return null;
            var steps=new Action[]{BuildScenery,BuildHome,BuildKeepy,BuildRooms,BuildBedrooms,BuildBedroomFurniture,BuildSecrets,BuildBooks,BuildRoomPlay,BuildKitchen,BuildDiscovery,BuildHideAndSeek,BuildPark,BuildTag,BuildPond,BuildBathroom,BuildCreekBoats,BuildCreekFishing,BuildZoo,BuildDinosaurWorld,BuildKingdom,BuildDaycare,BuildSandpit,BuildTreasure,BuildVet,BuildDaycarePlay,BuildSeagulls,BuildShore,BuildWaveRide,BuildMarkets};
            foreach(var step in steps)
            {
                BuildMeasured(step);loadingScreen.SetAsLastSibling();yield return null;
            }
            yield return PrepareRecoveredPresentation(CurrentArea);
            CaptureNewPresentationLayout();
            loadingScreen.gameObject.SetActive(false);presentationBuilding=false;presentationBuilder=null;
            Render();WritePresentationEvidence();
        }
        private IEnumerator PrepareRecoveredPresentation(string area)
        {
            if(preparedRecoveredWorlds.Contains(area))yield break;
            int tiles;Func<string,Texture2D> load;Action build;
            if(area==HulkBirthday.Zone && BirthdayArt.Available){tiles=BirthdayArt.Data.tileCount;load=BirthdayArt.Texture;build=BuildBirthday;}
            else if(area==SpideyHQ.Zone && SpideyArt.Available){tiles=SpideyArt.Data.tileCount;load=SpideyArt.Texture;build=BuildSpidey;}
            else if(area==PeppaFunfair.Zone && FunfairArt.Available){tiles=FunfairArt.Data.tileCount;load=FunfairArt.Texture;build=BuildFunfairFromWorld;}
            else yield break;
            // Background image decoding is the largest first-entry burst.
            // Warm one image per frame before creating this world's interface.
            for(int i=0;i<tiles;i++){load("world-"+i+".png");yield return null;}
            BuildMeasured(build);preparedRecoveredWorlds.Add(area);
            CaptureNewPresentationLayout();
            if(loadingScreen!=null && WorldLoading)loadingScreen.SetAsLastSibling();
            WritePresentationEvidence();
        }
        private void CaptureNewPresentationLayout()
        {
            foreach(RectTransform child in safe)
                if(child!=loadingScreen && child.gameObject.activeSelf && !layoutPositions.ContainsKey(child))layoutPositions[child]=child.anchoredPosition;
            ApplyResponsiveLayout();
        }
        private void WritePresentationEvidence()
        {
            var evidence=new BuildEvidence{build=Application.version,world=CurrentArea,shared=Shared,steps=presentationSteps.ToArray()};
            var path=Path.Combine(Application.persistentDataPath,"Performance","presentation-build.json");
            Task.Run(()=>{try{Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(evidence));}catch(IOException){}catch(UnauthorizedAccessException){}});
        }
    }
}
