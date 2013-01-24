<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="p4_changelist2.aspx.vb" Inherits="web_tools.p4_changelist2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Perforce Changelist Generator</title>    
    <link type="text/css" href="./styles.css" rel="stylesheet">
    <link type="text/css" href="./jquery/jquery-ui-1.8.1.custom.css" rel="stylesheet">
    <script type="text/javascript" language="javascript" src="./jquery/jquery.js"></script>
    <script type="text/javascript" language="javascript" src="./jquery/jquery-ui.js"></script>    
    <script type="text/javascript" language="javascript">
    $(function(){
        if ($("#txt_p4cl").val() == "") {
            $("#txt_p4cl").focus();   
            $("#tr_output1").hide();
            $("#tr_output2").hide();
        }          
	    else {
	        $("#txt_output").focus();
	        $("#tr_output1").show();
            $("#tr_output2").show();
	    }	 
	    
            
        $("#dialog").dialog({
            autoOpen: false,
            width: 300,
            height: 100,
            modal: true,
            resizable: false,
            draggable: false,            
            buttons: {
				"Ok": function() { 
					$(this).dialog("close"); 
					$("#txt_p4cl").focus();
				}
			},
			open: function(event, ui) { 
			    $(":button:contains('Ok')").focus();    // Set focus to the [Ok] button
			}
		});
		
		
		$("#txt_p4cl").keypress(function(event) {
            if (event.keyCode == '13') {
                event.preventDefault();
            }
        });
	});
    
    
    function isDigit(num) {
	    if (num.length>1){return false;}
	    var string="1234567890";
	    if (string.indexOf(num)!=-1){return true;}
	    return false;
	}
    
    function uf_IsInt(val) {	    
	    for(var i=0;i<val.length;i++) {
		    if(!isDigit(val.charAt(i))) {
		        return false;
		    }
		}
		
	    return true;
	}
    
    function uf_validate() {
        if (document.getElementById('txt_p4cl').value == '') {            
            $("#sp_dialog").text("Please enter changelist number.");
			$("#dialog").dialog("open");		
            return false;
        }
        else if (!uf_IsInt(document.getElementById('txt_p4cl').value)) {
            $("#sp_dialog").text("Changelist must be numeric.");
			$("#dialog").dialog("open");
            return false;
        }
        
        return true;
    }
    </script>
</head>

<body>
    <form id="form1" runat="server">
    <div id="dialog" title="Error">
        <p><span class="ui-icon ui-icon-alert" style="float: left; margin-right: .3em;"></span>
        <span id="sp_dialog" style="font-size:11px;"></span></p>
    </div>
    
    <br />
    <table width="80%" align="center" border="1" cellspacing="0" ID="Table1" class="formInput">
	    <tr>
		    <td>
		        <div style="text-align:center;"> <b>P4 Changelist Files Generator</b> </div> <br />
			    <table width="100%" ID="Table2">
				    <!-- INPUT SECTION -------------------------------------------------------------------------------------------------->
				    <tr>
					    <td style="width:22%">
						    Perforce Changelist Number:
					    </td>
				        <td>
					        <asp:TextBox runat="server" ID="txt_p4cl" MaxLength="10" style="text-align:right; width:100px;"></asp:TextBox>
					        <asp:Button runat="server" ID="bt_submit" Text="Generate" />
					        <asp:Button runat="server" ID="bt_clear" Text="Clear" />
					    </td>
				    </tr>							
    				
				    <!-- OUTPUT SECTION -------------------------------------------------------------------------------------------------->
				    <tr>
					    <td colspan="2" >						
						    <hr />
					    </td>
				    </tr>
				    <tr id="tr_output1">
					    <td colspan="2" >						
						    Output:
					    </td>
				    </tr>
				    <tr id="tr_output2">
					    <td colspan="2" align="center">						    
						    <asp:TextBox runat="server" ID="txt_output" style="width:99%;" Rows="20" TextMode="MultiLine"></asp:TextBox>
					    </td>
				    </tr>
			    </table>
		    </td>
	    </tr>
    </table>
    </form>
</body>
</html>
