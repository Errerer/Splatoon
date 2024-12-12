﻿using ECommons.PartyFunctions;
using System.Diagnostics.CodeAnalysis;
#nullable enable

namespace Splatoon.SplatoonScripting.Priority;
public class PriorityList
{
    internal string ID = GetTemporaryId();
    public List<JobbedPlayer> List = [];
    internal ImGuiEx.RealtimeDragDrop<JobbedPlayer> DragDrop;
    public bool IsRole = false;

    public PriorityList()
    {
        DragDrop = new(ID, x => x.ID);
    }

    internal void DrawModeSelector()
    {
        ImGuiEx.TextV("List mode:");
        ImGui.SameLine();
        ImGuiEx.RadioButtonBool("Roles", "Names and/or jobs", ref this.IsRole, true);
    }

    internal void Draw()
    {
        for(var q = 0; q < List.Count; q++)
        {
            var player = List[q];
            ImGui.PushID(player.ID);
            ImGui.TableNextRow();
            DragDrop.SetRowColor(player.ID);
            ImGui.TableNextColumn();
            DragDrop.NextRow();
            DragDrop.DrawButtonDummy(player, List, q);
            ImGui.TableNextColumn();
            player.DrawSelector(IsRole);
            if(IsRole)
            {
                ImGui.SameLine();
                if(player.IsInParty(true, out var resolved))
                {
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.Text(EColor.GreenBright, FontAwesomeIcon.Check.ToIconString());
                    ImGui.PopFont();
                    ImGui.SameLine();
                    ImGuiEx.Text($"Resolved to: {resolved.NameWithWorld} | {resolved.ClassJob}");
                }
                else
                {
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.Text(EColor.RedBright, FontAwesomeIcon.Times.ToIconString());
                    ImGui.PopFont();
                    ImGui.SameLine();
                    ImGuiEx.Text($"Not resolved");
                }
            }
            ImGui.TableNextColumn();
            if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                player.Name = "";
                player.Jobs.Clear();
                player.Role = RolePosition.Not_Selected;
            }
            ImGui.PopID();
        }
    }

    public bool Test([NotNullWhen(false)] out string? error)
    {
        error = null;
        if(!IsRole)
        {
            if(List.Any(x => x.Name == "" && x.Jobs.Count == 0))
            {
                error = "There are unfilled slots in this priority list.";
                return false;
            }
        }
        else
        {
            if(List.Any(x => x.Role == RolePosition.Not_Selected))
            {
                error = "There are unfilled slots in this priority list.";
                return false;
            }
        }
        var exist = new List<UniversalPartyMember>();
        foreach(var x in List)
        {
            if(x.IsInParty(this.IsRole, out var member))
            {
                if(exist.Contains(member))
                {
                    error = "One or more entries matches multiple players.";
                }
                else
                {
                    exist.Add(member);
                }
            }
            else
            {
                error = "Current party does not matches this priority list.";
            }
        }
        return error == null;
    }
}
