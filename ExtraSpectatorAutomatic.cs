/* ExtraSpectatorAutomatic.cs
 
 * Copyright 2014 by Schwitz Markus ( MarkusSR1984 ) schwitz@sossau.com
 *
 * thanks a lot to a few Plugin Develooper for some help in Forum and with ther Plugins where i could
 * read or copy some code to understand how ProCon Plugins working. 
 * 
 * Extra Spectator Automatic is free software: you can redistribute it and/or modify it under the terms of the
 * GNU General Public License as published by the Free Software Foundation, either version 3 of the License,
 * or (at your option) any later version. Extra Spectator Automatic is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. You should have received a copy of the
 * GNU General Public License along with ExtraServerFuncs. If not, see http://www.gnu.org/licenses/.

*/

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Web;
using System.Data;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Reflection;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;


namespace PRoConEvents
{

//Aliases
using EventType = PRoCon.Core.Events.EventType;
using CapturableEvent = PRoCon.Core.Events.CapturableEvents;

public class ExtraSpectatorAutomatic : PRoConPluginAPI, IPRoConPluginInterface
{


private bool isPluginEnabled;
private int DebugLevel;
private enumBoolYesNo force_spectators;
private enumBoolYesNo allow_public_spectators;
private int enable_spectators_PlayerCount;
private int disable_spectators_PlayerCount;
private int spectator_PortsCount;
private int playerCount;
private int maxPlayerCount;
private bool isSectatorEnabled = true;

private Hashtable PluginInfo = new Hashtable();
private bool isRegisteredInTaskPlaner = false;
private List<string> Commands = new List<string>();
private List<string> Variables = new List<string>();






public ExtraSpectatorAutomatic() {
	isPluginEnabled = false;
	DebugLevel = 2;
    force_spectators = enumBoolYesNo.No;
    allow_public_spectators = enumBoolYesNo.Yes;
    enable_spectators_PlayerCount = 15;
    disable_spectators_PlayerCount = 20;
    spectator_PortsCount = 2;
    playerCount = 0;
    maxPlayerCount = 16;

    
}

public void WritePluginConsole(string message, string tag, int level)
{
    try
    {

        if (tag == "ERROR")
        {
            tag = "^1" + tag;   // RED
        }
        else if (tag == "DEBUG")
        {
            tag = "^3" + tag;   // ORAGNE
        }
        else if (tag == "INFO")
        {
            tag = "^2" + tag;   // GREEN
        }
        else if (tag == "VARIABLE")
        {
            tag = "^6" + tag;   // GREEN
        }
        else if (tag == "WARN")
        {
            tag = "^7" + tag;   // PINK
        }


        else
        {
            tag = "^5" + tag;   // BLUE
        }

        string line = "^b[" + this.GetPluginName() + "] " + tag + ": ^0^n" + message;


        if (tag == "ENABLED") line = "^b^2" + line;
        if (tag == "DISABLED") line = "^b^3" + line;

        //if (this.fDebugLevel >= 3) // WRITE LOG FILE
        //{
        //    files.DebugWrite(LogFileName, Regex.Replace("[" + DateTime.Now + "]" + line, "[/^][0-9bni]", "")); // Lösche formatierung und schreibe in Logdatei
        //}


        if (this.DebugLevel >= level)
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", line);
        }

    }
    catch (Exception e)
    {
        this.ExecuteCommand("procon.protected.pluginconsole.write", "^1^b[" + this.GetPluginName() + "][ERROR]^n WritePluginConsole: ^0" + e);
    }

}


public void ServerCommand(params String[] args)
{
	List<string> list = new List<string>();
	list.Add("procon.protected.send");
	list.AddRange(args);
	this.ExecuteCommand(list.ToArray());
}


public string GetPluginName() {
	return "Extra Spectator Automatic";
}

public string GetPluginVersion() {
	return "1.0.0.0";
}

public string GetPluginAuthor() {
	return "MarkusSR1984";
}

public string GetPluginWebsite() {
    return "github.com/GladiusGloriae/ExtraSpectatorAutomatic.git";
}

public string GetPluginDescription() {
	return @"

If you find this plugin useful, please consider supporting me. Donations help support the servers used for development and provide incentive for additional features and new plugins! Any amount would be appreciated!</p>

<center>
<form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
<input type=""hidden"" name=""cmd"" value=""_s-xclick"">
<input type=""hidden"" name=""hosted_button_id"" value=""4VYFL94U9ME8L"">
<input type=""image"" src=""https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif"" border=""0"" name=""submit"" alt=""PayPal - The safer, easier way to pay online!"">
<img alt="""" border=""0"" src=""https://www.paypalobjects.com/de_DE/i/scr/pixel.gif"" width=""1"" height=""1"">
</form>
</center>


<h2>Description</h2>
<p>This Plugin Enables or Disables Spectator Ports based on Player Count</p>

<h2>Commands</h2>
<p>This Plugin has no Commands</p>

<h2>Settings</h2>
<p>
<blockquote><h4>Enable Spectators Player Count</h4>
On how many or less Players should Spectator Ports get activated<br/>
</blockquote>
<blockquote><h4>Disable Spectators Player Count</h4>
On how many or more Players should Spectator Ports get deactivated<br/>
</blockquote>
<blockquote><h4>Spectator Ports Count</h4>
Set the Count of Spectator Ports<br/>
</blockquote>
<blockquote><h4>Debug level</h4>
Set the Debug level. Default is 2<br/>
</blockquote>



</p>

<h3>Changelog</h3>
<blockquote><h4>1.0.0.1 (13-04-2014)</h4>
	- Added Support for Extra Task Manager<br/>
</blockquote>

<blockquote><h4>1.0.0.0 (24-02-2014)</h4>
	- initial version<br/>
</blockquote>
";
}




private void EnableSpectators()
{
    if (!isSectatorEnabled) WritePluginConsole("Enable Spectators", "INFO", 2);
    isSectatorEnabled = true;
    if (allow_public_spectators == enumBoolYesNo.Yes) ServerCommand("vars.alwaysAllowSpectators", "true");
    ServerCommand("vars.maxSpectators", spectator_PortsCount.ToString() );
}


private void DisableSpectators()
{
    if (isSectatorEnabled) WritePluginConsole("Disable Spectators", "INFO", 2);
    isSectatorEnabled = false;
    ServerCommand("vars.alwaysAllowSpectators", "false");
    ServerCommand("vars.maxSpectators", "0");
}

public bool IsExtraTaskPlanerInstalled()
{
    List<MatchCommand> registered = this.GetRegisteredCommands();
    foreach (MatchCommand command in registered)
    {
        if (command.RegisteredClassname.CompareTo("ExtraTaskPlaner") == 0 && command.RegisteredMethodName.CompareTo("PluginInterface") == 0)
        {
            WritePluginConsole("^bExtra Task Planer^n detected", "INFO", 3);
            return true;
        }

    }

    return false;
}

public void ExtraTaskPlaner_Callback(string command)
{

    if (command == "success")
    {
        isRegisteredInTaskPlaner = true;
    }

    
}


private string GetCurrentClassName()
{
    string tmpClassName;

    tmpClassName = this.GetType().ToString(); // Get Current Classname String
    tmpClassName = tmpClassName.Replace("PRoConEvents.", "");


    return tmpClassName;

}


private void SendTaskPlanerInfo()
{



    Variables.Add("Enable Spectators Player Count");
    Variables.Add("Disable Spectators Player Count");
    Variables.Add("Spectator Ports Count");




    PluginInfo["PluginName"] = GetPluginName();
    PluginInfo["PluginVersion"] = GetPluginVersion();
    PluginInfo["PluginClassname"] = GetCurrentClassName();

    PluginInfo["PluginCommands"] = CPluginVariable.EncodeStringArray(Commands.ToArray());
    PluginInfo["PluginVariables"] = CPluginVariable.EncodeStringArray(Variables.ToArray());

    this.ExecuteCommand("procon.protected.plugins.setVariable", "ExtraTaskPlaner", "RegisterPlugin", JSON.JsonEncode(PluginInfo)); // Send Plugin Infos to Task Planer

}



public List<CPluginVariable> GetDisplayPluginVariables() {

	List<CPluginVariable> lstReturn = new List<CPluginVariable>();

	lstReturn.Add(new CPluginVariable("Special|Debug level", DebugLevel.GetType(), DebugLevel));


  //  lstReturn.Add(new CPluginVariable("General|Force Spectator Ports", typeof(enumBoolYesNo), force_spectators));
  // lstReturn.Add(new CPluginVariable("General|Allow Public Spectators", typeof(enumBoolYesNo), allow_public_spectators));
    lstReturn.Add(new CPluginVariable("General|Enable Spectators Player Count", typeof(int), enable_spectators_PlayerCount));
    lstReturn.Add(new CPluginVariable("General|Disable Spectators Player Count", typeof(int), disable_spectators_PlayerCount));
    lstReturn.Add(new CPluginVariable("General|Spectator Ports Count", typeof(int), spectator_PortsCount));

       
	return lstReturn;
}

public List<CPluginVariable> GetPluginVariables() {
	return GetDisplayPluginVariables();
}

public void SetPluginVariable(string strVariable, string strValue) {

    if (Regex.Match(strVariable, @"ExtraTaskPlaner_Callback").Success)
    {
        ExtraTaskPlaner_Callback(strValue);
    }

    
    
    
    if (Regex.Match(strVariable, @"Allow Public Spectators").Success)
    {
        enumBoolYesNo tmp = enumBoolYesNo.Yes;

        if (strValue == "No") tmp = enumBoolYesNo.No;
        allow_public_spectators = tmp;
        
    }



    
    if (Regex.Match(strVariable, @"Enable Spectators Player Count").Success)
    {
        
        int tmp = 15;
		int.TryParse(strValue, out tmp);
        enable_spectators_PlayerCount = tmp;
                
    }

    if (Regex.Match(strVariable, @"Disable Spectators Player Count").Success)
    {

        int tmp = 20;
        int.TryParse(strValue, out tmp);
        disable_spectators_PlayerCount = tmp;

    }

    if (Regex.Match(strVariable, @"Spectator Ports Count").Success)
    {
        
        int tmp = 2;
        int.TryParse(strValue, out tmp);
        spectator_PortsCount = tmp;

    }

    if (Regex.Match(strVariable, @"Debug level").Success)
    {
        int tmp = 2;
        int.TryParse(strValue, out tmp);
        DebugLevel = tmp;
    }


}


public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {
    this.RegisterEvents(this.GetType().Name, "OnServerInfo");
    
    Thread startup_sleep = new Thread(new ThreadStart(delegate()
    {
        Thread.Sleep(2000);
        if (IsExtraTaskPlanerInstalled())
        {
            do
            {
                SendTaskPlanerInfo();
                Thread.Sleep(2000);
            }
            while (!isRegisteredInTaskPlaner);
        }
    }));
    startup_sleep.Start();   




    //this.RegisterEvents(this.GetType().Name, "OnVersion", "OnServerInfo", "OnResponseError", "OnListPlayers", "OnPlayerJoin", "OnPlayerLeft", "OnPlayerKilled", "OnPlayerSpawned", "OnPlayerTeamChange", "OnGlobalChat", "OnTeamChat", "OnSquadChat", "OnRoundOverPlayers", "OnRoundOver", "OnRoundOverTeamScores", "OnLoadingLevel", "OnLevelStarted", "OnLevelLoaded");
}

public void OnPluginEnable() {
	isPluginEnabled = true;
	WritePluginConsole("Enabled!","INFO",0);
}

public void OnPluginDisable() {
	isPluginEnabled = false;
    WritePluginConsole("Disabled!", "INFO", 0);
}


public override void OnServerInfo(CServerInfo serverInfo) {
    
    playerCount = serverInfo.PlayerCount;
    maxPlayerCount = serverInfo.MaxPlayerCount;

    if (isPluginEnabled)
    {
        if (playerCount >= disable_spectators_PlayerCount) DisableSpectators();
        if (playerCount <= enable_spectators_PlayerCount) EnableSpectators();
    }
}

//public override void OnVersion(string serverType, string version) { }

//public override void OnResponseError(List<string> requestWords, string error) { }

//public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset) { }

//public override void OnPlayerJoin(string soldierName) { }

//public override void OnPlayerLeft(CPlayerInfo playerInfo) { }

//public override void OnPlayerKilled(Kill kKillerVictimDetails) { }

//public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory) { }

//public override void OnPlayerTeamChange(string soldierName, int teamId, int squadId) { }

//public override void OnGlobalChat(string speaker, string message) { }

//public override void OnTeamChat(string speaker, string message, int teamId) { }

//public override void OnSquadChat(string speaker, string message, int teamId, int squadId) { }

//public override void OnRoundOverPlayers(List<CPlayerInfo> players) { }

//public override void OnRoundOverTeamScores(List<TeamScore> teamScores) { }

//public override void OnRoundOver(int winningTeamId) { }

//public override void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal) { }

//public override void OnLevelStarted() { }

//public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal) { } // BF3


} // end ExtraSpectatorAutomatic

} // end namespace PRoConEvents



