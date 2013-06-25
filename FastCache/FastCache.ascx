<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FastCache.ascx.cs" Inherits="FastCache.FastCache" %>

<script>

    $(function () {
        $('.clearCache').click(function () {
            if (confirm('Are you sure you wish to clear the cache?')) {
                __doPostBack();
            }
        });
    });

</script>

<h2>Fast Cache</h2>

<div>
    <p>There are <span id="numPages" runat="server"></span> page(s) cached.</p>
    
    <input id='clearCache' type="button" class="clearCache" value="Clear Cache">
</div>