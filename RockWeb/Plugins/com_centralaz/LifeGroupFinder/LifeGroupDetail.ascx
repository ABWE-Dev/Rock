﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LifeGroupDetail.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.LifeGroupFinder.LifeGroupDetail" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <script type="text/javascript">
            function jumpToControl() {
                var $item = $("[id$='tbFirstName']");
                if ($item.length > 0) {
                    $('html, body').animate({
                        scrollTop: $item.offset().top
                    }, 1000);
                }
            };
        </script>
        <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" CssClass="lifegroupdetail-noticebox" NotificationBoxType="Danger" />
        <asp:Panel ID="pnlEmailSent" runat="server" Visible="false">
            <h1>Your request for information has been sent.
            </h1>
        </asp:Panel>
        <asp:Panel ID="pnlView" runat="server" Visible="true">
            <div class="row">
                    <Rock:RockLiteral ID="lGroupName" runat="server" CssClass="lifegroupdetail-groupname"/>
            </div>
            <hr />
            <div class="row">
                <div class="lifegroupdetail-backdiv pull-left">
                    <asp:LinkButton ID="lbGoBack" runat="server" Text="Back" OnClick="lbGoBack_Click" />
                </div>
                <asp:Panel ID="pnlLogin" runat="server" CssClass="lifegroupdetail-loginbutton">
                    <asp:LinkButton ID='lbLogin' runat='server' Text='Sign in' OnClick='lbLogin_Click' CausesValidation="false" />
                    to autocomplete forms. 
                </asp:Panel>
            </div>
            <div class="row">
                <div class="lifegroupdetail-videoboxdiv col-md-4 text-center">
                    <asp:Literal ID="lMainMedia" runat="server" />
                </div>
                <div class="lifegroupdetail-contactdiv col-md-4">
                    <div class="lifegroupdetail-registerdiv row text-center">
                        <asp:LinkButton ID="lbRegister" runat="server" Text="Sign up!" CssClass="btn btn-primary" OnClick="lbRegister_Click" CausesValidation="false" />
                    </div>
                    <div class="lifegroupdetail-emailleaderdiv row text-center">
                        <asp:LinkButton ID="lbEmail" runat="server" Text="Email" OnClick="lbEmail_Click" CausesValidation="false" />
                    </div>
                </div>
                <div class="lifegroupdetail-mapdiv col-md-4 location-maps text-center">
                    <asp:PlaceHolder ID="phMaps" runat="server" EnableViewState="true" />
                </div>
            </div>
            <div class="lifegroupdetail-descriptiondiv row">
                <asp:Literal ID="lDescription" runat="server" />
            </div>

            <asp:Literal ID="lPhotoOutput" runat="server" />
            <asp:Literal ID="lPhotoDebug" Visible="false" runat="server" />

            <div class="lifegroupdetail-validationdiv">
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlSignup" runat="server" Visible="true">
            <div class="lifegroupdetail-errordiv">
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />
            </div>

            <div class="lifegroupdetail-firstnamediv row">
                <div class="col-md-6">
                    <Rock:RockLiteral ID="lFirstName" runat="server" Text="We treat Life Groups like family, and to us family uses real names." />
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true"></Rock:RockTextBox>
                </div>
            </div>
            <div class="lifegroupdetail-lastnamediv row">
                <div class="col-md-6">
                    <Rock:RockLiteral ID="lLastName" runat="server" Text="What is yours?" />
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true"></Rock:RockTextBox>
                </div>
            </div>
            <div class="lifegroupdetail-homephonediv row">
                <div class="col-md-6">
                    <Rock:RockLiteral ID="lHome" runat="server" Text="Please provide your email and phone." />
                </div>
                <div class="col-md-6">
                    <Rock:PhoneNumberBox ID="pnHome" runat="server" Label="Home Phone" />
                </div>
            </div>
            <div class="lifegroupdetail-emaildiv row">
                <div class="col-md-6">
                    <Rock:RockLiteral ID="lEmail" runat="server" Text="To connect you to your selected group we would like to contact you." />
                </div>
                <div class="col-md-6">
                    <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="true"></Rock:EmailBox>
                </div>
            </div>
            <div class="lifegroupdetail-secondsignupdiv row">
                <div class="col-md-6">
                    <Rock:RockLiteral ID="lSecondSignup" runat="server" Text="Will some one else be attending with you, such as a spouse, friend, or neighbor?" />
                </div>
                <div class="col-md-6">
                    <Rock:RockCheckBox ID="cbSecondSignup" runat="server" Text="Yes" AutoPostBack="true" OnCheckedChanged="cbSecondSignup_CheckedChanged" />
                </div>
            </div>

            <asp:Panel ID="pnlSecondSignup" runat="server" Visible="false">
                <div class="lifegroupdetail-secondfirstnamediv row">
                    <div class="col-md-6">
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbSecondFirstName" runat="server" Label="First Name" Required="true"></Rock:RockTextBox>
                    </div>
                </div>
                <div class="lifegroupdetail-secondlastnamediv row">
                    <div class="col-md-6">
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbSecondLastName" runat="server" Label="Last Name" Required="true"></Rock:RockTextBox>
                    </div>
                </div>
                <div class="lifegroupdetail-secondhomephonediv row">
                    <div class="col-md-6">
                    </div>
                    <div class="col-md-6">
                        <Rock:PhoneNumberBox ID="pnSecondHome" runat="server" Label="Home Phone" />
                    </div>
                </div>
                <div class="lifegroupdetail-secondemaildiv row">
                    <div class="col-md-6">
                    </div>
                    <div class="col-md-6">
                        <Rock:EmailBox ID="tbSecondEmail" runat="server" Label="Email" Required="true"></Rock:EmailBox>
                    </div>
                </div>
            </asp:Panel>

            <div class="pull-right">
                <div class="lifegroupdetail-actiondiv actions">
                    <asp:LinkButton ID="btnRegister" runat="server" Text="Sign up!" CssClass="btn btn-primary" OnClick="btnRegister_Click" />
                    <asp:LinkButton ID="btnEmail" runat="server" Text="Send Email" CssClass="btn btn-primary" OnClick="btnEmail_Click" Visible="false" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlResult" runat="server" Visible="false">
            <center>
            <h3>Success!</h3>
            <hr />
            <h3>
                Thank you for submitting your info!
                <br />
                You should recieve a confirmation email shortly.
            </h3>
            </center>
            <div class="col-md-6 col-md-offset-3">
                <asp:Panel runat="server" CssClass="lifegroupdetail-firstpersonresult">
                    <div class="row">
                        <Rock:RockTextBox ID="tbResultFirstName" runat="server" Enabled="false"></Rock:RockTextBox>
                    </div>
                    <div class="row">
                        <Rock:RockTextBox ID="tbResultLastName" runat="server" Enabled="false"></Rock:RockTextBox>
                    </div>
                    <div class="row">
                        <Rock:EmailBox ID="tbResultEmail" runat="server" Enabled="false"></Rock:EmailBox>
                    </div>
                    <div class="row">
                        <Rock:PhoneNumberBox ID="pnResultHome" runat="server" Enabled="false" />
                    </div>
                </asp:Panel>
                <br />
                <asp:Panel ID="pnlSecondResult" runat="server" Visible="false" CssClass="lifegroupdetail-secondpersonresult">
                    <div class="row">
                        <Rock:RockTextBox ID="tbSecondResultFirstName" runat="server" Enabled="false"></Rock:RockTextBox>
                    </div>
                    <div class="row">
                        <Rock:RockTextBox ID="tbSecondResultLastName" runat="server" Enabled="false"></Rock:RockTextBox>
                    </div>
                    <div class="row">
                        <Rock:EmailBox ID="tbSecondResultEmail" runat="server" Enabled="false"></Rock:EmailBox>
                    </div>
                    <div class="row">
                        <Rock:PhoneNumberBox ID="pnSecondResultHome" runat="server" Enabled="false" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
