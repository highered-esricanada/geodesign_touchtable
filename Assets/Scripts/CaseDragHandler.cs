using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using TouchScript;
using TablePlus.ElementsDB.DBBridge;
using TMPro;

public class CaseDragHandler : MonoBehaviour
{


    private DBCase GetCaseInfo(string name)
    {

       // gets the file path that hold all information on the cases
        string filePath = Path.Combine(Application.streamingAssetsPath, "cases.json");

        //if it exists:
        if (File.Exists(filePath))
        {
            //initialize new string
            string dataAsJson;

            //reads all the info from cases.json
            dataAsJson = File.ReadAllText(filePath);
            //deserializes the text file to a JSON array type
            var cases = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(dataAsJson);
            
            //for each of these, get the "_casenumber", if it equals the name from the method parameter,
            //calls a method in DBBridge, which gets the db case value
            foreach (JObject dbcase in cases)
            {
                if (dbcase.GetValue("_CaseNumber").ToString().Equals(name))
                {
                    return Elements.GetCaseByID(dbcase.GetValue("_CaseId").ToString());
                }
            }

        }
        else
        {
            Debug.LogError("Cannot load task data!");
        }

        return null;


    }


    public void GenerateCaseInfo()
    {
        DBCase dbcase = GetCaseInfo(this.transform.parent.gameObject.name);

        //get needed properties
        string name, caseNumber, caption, caseID;
        dbcase.Properties.TryGetValue("case_name", out name);
        dbcase.Properties.TryGetValue("case_number", out caseNumber);
        dbcase.Properties.TryGetValue("caption", out caption);
        dbcase.Properties.TryGetValue("case_id", out caseID);

        string far, uph, hect;
        dbcase.Properties.TryGetValue("cal_FAR", out far);
        dbcase.Properties.TryGetValue("cal_UPH", out uph);
        dbcase.Properties.TryGetValue("par_site_area", out hect);
        hect = (float.Parse(hect) / 10000).ToString();

        if (transform.Find("ScrollInfoControl") == null)
        {
            GameObject scrollInfoControl = (GameObject)Instantiate(Resources.Load("ScrollInfoControl"));
            scrollInfoControl.transform.SetParent(this.transform);
            scrollInfoControl.transform.name = "ScrollInfoControl";
            RectTransform rt = scrollInfoControl.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(0, 1f, 0);
            rt.offsetMax = new Vector2(0, 200f);
            rt.offsetMin = new Vector2(150f, 0);

            scrollInfoControl.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            //scrollInfoControl.transform.eulerAngles = transform.Find("CaseControlMenu").transform.eulerAngles;
            scrollInfoControl.transform.eulerAngles = new Vector3(90,0,0);


            //scrollInfoControl.transform.Find("Title").Find("Name").GetComponent<TextMeshProUGUI>().SetText(name);
            scrollInfoControl.transform.Find("Title").Find("Name").GetComponent<Text>().text = name;
            scrollInfoControl.transform.Find("Title").Find("CaseNumber").GetComponent<Text>().text = caseNumber;
            scrollInfoControl.transform.Find("Title").Find("Caption").GetComponent<Text>().text = caption;

            scrollInfoControl.transform.Find("GeneralDescribe").Find("FloorAreaRatioValue").GetComponent<Text>().text = far + "FAR";
            scrollInfoControl.transform.Find("GeneralDescribe").Find("HousingDensityValue").GetComponent<Text>().text = uph + "UPH";
            scrollInfoControl.transform.Find("GeneralDescribe").Find("SiteAreaValue").GetComponent<Text>().text = hect + " hect";

            scrollInfoControl.transform.Find("3DModelView").GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/" + caseNumber);


            string UniqueCaseNumber, LandUse = "";
            string Jobs, Population;
            string FloorAreaRatio, DwellingDensity, SiteArea, SiteWidth, SiteDepth, EffectiveImpervious, BoundingBoxWidth, BoundingBoxDepth;
            string MaxFloors, TotalConditionedArea, ResidentialArea, CommercialArea, CivicArea, IndustrialArea, OtherArea, ParkingArea;
            string Dwellings, Bedrooms, Bathrooms, CommercialUnits;

            UniqueCaseNumber = caseNumber;

            foreach (DBLanduse dblanduse in dbcase.LanduseList)
            {
                LandUse += dblanduse._Type + "|";
            }

            dbcase.Properties.TryGetValue("cal_FAR", out FloorAreaRatio);
            dbcase.Properties.TryGetValue("cal_UPH", out DwellingDensity);
            dbcase.Properties.TryGetValue("par_site_area", out SiteArea);
            dbcase.Properties.TryGetValue("par_avg_site_width", out SiteWidth);
            dbcase.Properties.TryGetValue("par_avg_site_depth", out SiteDepth);
            dbcase.Properties.TryGetValue("cal_s_site_effective_impervious", out EffectiveImpervious);

            BoundingBoxWidth = this.transform.localScale.x.ToString();
            BoundingBoxDepth = this.transform.localScale.y.ToString();


            dbcase.Properties.TryGetValue("cal_T_f_max_occupied_stories", out MaxFloors);
            dbcase.Properties.TryGetValue("cal_T_f_total_cond_residential", out ResidentialArea);
            dbcase.Properties.TryGetValue("cal_T_f_total_cond_commercial", out CommercialArea);
            dbcase.Properties.TryGetValue("cal_T_f_total_cond_civic", out CivicArea);
            dbcase.Properties.TryGetValue("cal_T_f_total_cond_industrial", out IndustrialArea);
            dbcase.Properties.TryGetValue("cal_T_f_total_cond_other", out OtherArea);
            dbcase.Properties.TryGetValue("cal_T_f_structured_parking", out ParkingArea);
            TotalConditionedArea = (double.Parse((CivicArea == null) ? "0" : CivicArea) + double.Parse((CommercialArea == null) ? "0" : CommercialArea)
                + double.Parse((IndustrialArea == null) ? "0" : IndustrialArea) + double.Parse((OtherArea == null) ? "0" : OtherArea) + double.Parse((ResidentialArea == null) ? "0" : ResidentialArea)).ToString();

            //(int)((double)CaseFacts[CaseFactKeys.CommercialArea]/33
            Jobs = ((int)(double.Parse((CommercialArea == null) ? "0" : CommercialArea) / 33)).ToString();
            //(int)((double)CaseFacts[CaseFactKeys.ResidentialArea]/56)
            Population = ((int)(double.Parse((ResidentialArea == null) ? "0" : ResidentialArea) / 56)).ToString();


            dbcase.Properties.TryGetValue("cal_T_total_residential_units", out Dwellings);
            dbcase.Properties.TryGetValue("cal_T_u_residential_bedrooms", out Bedrooms);
            dbcase.Properties.TryGetValue("cal_T_u_residential_bathrooms", out Bathrooms);
            dbcase.Properties.TryGetValue("cal_T_total_commercial_units", out CommercialUnits);


            //Load case attributes
            Transform content = scrollInfoControl.transform.Find("CaseAttributes").Find("Content_Cases");

            content.Find("Case Number Value").GetComponent<Text>().text = (UniqueCaseNumber == null) ? "0" : UniqueCaseNumber;
            content.Find("Land Use Value").GetComponent<Text>().text = (LandUse == null) ? "0" : LandUse;

            content.Find("Jobs Value").GetComponent<Text>().text = (Jobs == null) ? "0" : Jobs; ;
            content.Find("Population Value").GetComponent<Text>().text = (Population == null) ? "0" : Population;

            content.Find("Floor Area Ratio Value").GetComponent<Text>().text = (FloorAreaRatio == null) ? "0" : FloorAreaRatio;
            content.Find("Dewlling Density Value").GetComponent<Text>().text = (DwellingDensity == null) ? "0" : DwellingDensity;
            content.Find("Site Area Value").GetComponent<Text>().text = (SiteArea == null) ? "0" : SiteArea;
            content.Find("Site Width Value").GetComponent<Text>().text = (SiteWidth == null) ? "0" : SiteWidth;
            content.Find("Site Depth Value").GetComponent<Text>().text = (SiteDepth == null) ? "0" : SiteDepth;
            content.Find("Effective Impervious Value").GetComponent<Text>().text = (EffectiveImpervious == null) ? "0" : EffectiveImpervious;
            content.Find("Bounding Box Width Value").GetComponent<Text>().text = (BoundingBoxWidth == null) ? "0" : BoundingBoxWidth;
            content.Find("Bounding Box Depth Value").GetComponent<Text>().text = (BoundingBoxDepth == null) ? "0" : BoundingBoxDepth;

            content.Find("Max Floors Value").GetComponent<Text>().text = (MaxFloors == null) ? "0" : MaxFloors;
            content.Find("Total Conditioned Area Value").GetComponent<Text>().text = (TotalConditionedArea == null) ? "0" : TotalConditionedArea;
            content.Find("Residential Area Value").GetComponent<Text>().text = (ResidentialArea == null) ? "0" : ResidentialArea;
            content.Find("Commercial Area Value").GetComponent<Text>().text = (CommercialArea == null) ? "0" : CommercialArea;
            content.Find("Civic Area Value").GetComponent<Text>().text = (CivicArea == null) ? "0" : CivicArea;
            content.Find("Industrial Area Value").GetComponent<Text>().text = (IndustrialArea == null) ? "0" : IndustrialArea;
            content.Find("Other Area Value").GetComponent<Text>().text = (OtherArea == null) ? "0" : OtherArea;
            content.Find("Parking Area Value").GetComponent<Text>().text = (ParkingArea == null) ? "0" : ParkingArea;

            content.Find("Dwellings Value").GetComponent<Text>().text = (Dwellings == null) ? "0" : Dwellings;
            content.Find("Bedrooms Value").GetComponent<Text>().text = (Bedrooms == null) ? "0" : Bedrooms;
            content.Find("Bathrooms Value").GetComponent<Text>().text = (Bathrooms == null) ? "0" : Bathrooms;
            content.Find("Commercial Units Value").GetComponent<Text>().text = (CommercialUnits == null) ? "0" : CommercialUnits;



            foreach (DBCaseResource dbcaseresource in dbcase.ResourceList)
            {
                if (dbcaseresource._Extension != "skp")
                    scrollInfoControl.transform.Find("Viewport").Find("Content").gameObject.GetComponent<LoadAdditionalImages>().loadAdditionalImage(dbcaseresource);
            }

        }
    }



}
