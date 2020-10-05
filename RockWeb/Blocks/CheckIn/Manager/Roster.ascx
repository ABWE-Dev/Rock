﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Roster.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.Roster" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-cancel-checkin').on('click', function (event) {
            event.stopImmediatePropagation();
            var personName = $(this).parent().siblings(".js-name").find(".js-checkin-person-name").first().text();
            return Rock.dialogs.confirmDelete(event, 'Check-in for ' + personName);
        });
    });
</script>
<Rock:RockUpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlContent" runat="server" CssClass="checkin-roster">

            <div class="page-title-addon d-flex flex-wrap justify-content-between align-items-center">
                <div>
                    <Rock:LocationPicker ID="lpLocation" runat="server" AllowedPickerModes="Named" CssClass="picker-lg" OnSelectLocation="lpLocation_SelectLocation" IncludeInactiveNamedLocations="true" />
                </div>
                <asp:Panel ID="pnlSubPageNav" runat="server" class="">
                    <Rock:PageNavButtons ID="pbSubPages" runat="server" IncludeCurrentQueryString="true" />
                </asp:Panel>
            </div>

            <asp:Panel ID="pnlRoster" runat="server" CssClass="panel panel-block">
                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">Room Roster</h1>
                    <div class="pull-right">
                        <Rock:ButtonGroup ID="bgStatus" runat="server" FormGroupCssClass="toggle-container" SelectedItemClass="btn btn-primary active" UnselectedItemClass="btn btn-default" AutoPostBack="true" OnSelectedIndexChanged="bgStatus_SelectedIndexChanged">
                            <asp:ListItem Text="All" Value="1" />
                            <asp:ListItem Text="Checked-in" Value="2" />
                            <asp:ListItem Text="Present" Value="3" />
                        </Rock:ButtonGroup>
                    </div>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gAttendees" runat="server" DisplayType="Light" UseFullStylesForLightGrid="true" OnRowDataBound="gAttendees_RowDataBound" OnRowSelected="gAttendees_RowSelected" DataKeyNames="PersonGuid,AttendanceIds">
                            <Columns>
                                <Rock:RockLiteralField ID="lPhoto" ItemStyle-CssClass="avatar-column" ColumnPriority="TabletSmall" />
                                <Rock:RockLiteralField ID="lMobileIcon" HeaderStyle-CssClass="d-sm-none" ItemStyle-CssClass="mobile-icon d-table-cell d-sm-none" />
                                <Rock:RockLiteralField ID="lName" HeaderText="Name" ItemStyle-CssClass="name js-name" />
                                <Rock:RockLiteralField ID="lBadges" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="badges d-none d-sm-table-cell align-middle" />
                                <Rock:RockBoundField DataField="Tag" HeaderText="Tag" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="tag d-none d-sm-table-cell align-middle" />
                                <Rock:RockBoundField DataField="ServiceTimes" HeaderText="Service Times" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="service-times d-none d-sm-table-cell align-middle" />
                                <Rock:RockLiteralField ID="lMobileTagAndSchedules" HeaderText="Tag & Schedules" HeaderStyle-CssClass="d-sm-none" ItemStyle-CssClass="tags-and-schedules d-table-cell d-sm-none" />
                                <Rock:RockLiteralField ID="lCheckInTime" HeaderText="Check-in Time" HeaderStyle-HorizontalAlign="Right" ItemStyle-CssClass="check-in-time" ItemStyle-HorizontalAlign="Right" ColumnPriority="TabletSmall" />
                                <Rock:RockLiteralField ID="lStatusTag"  ItemStyle-CssClass="status-tag d-none d-sm-table-cell align-middle" ItemStyle-HorizontalAlign="Right" ColumnPriority="TabletSmall" />
                                <Rock:LinkButtonField ID="lbCancel" ItemStyle-CssClass="" CssClass="btn btn-default js-cancel-checkin" OnClick="lbCancel_Click" />
                                <Rock:LinkButtonField ID="lbPresent" ItemStyle-CssClass="" CssClass="btn btn-success" OnClick="lbPresent_Click" />
                                <Rock:LinkButtonField ID="lbCheckOut" ItemStyle-CssClass="" CssClass="btn btn-primary" OnClick="lbCheckOut_Click" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </asp:Panel>

        </asp:Panel>

    </ContentTemplate>
</Rock:RockUpdatePanel>