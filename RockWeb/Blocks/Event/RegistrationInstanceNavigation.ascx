﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceNavigation.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceNavigation" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <ul class="nav nav-pills margin-b-md">
            <asp:Repeater ID="rptPages" runat="server" OnItemDataBound="rptPages_ItemDataBound">
                <ItemTemplate>
                    <li runat="server" id="liNavigationTab" class="">
                        <a runat="server" id="aPageLink">
                            <asp:Literal ID="lPageName" runat="server" /></a>
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>

    </ContentTemplate>
</asp:UpdatePanel>
