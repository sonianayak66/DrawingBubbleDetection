var verifiedDiv = document.getElementById("verifyDiv");
console.log(verifiedDiv);
function DemandVerification() {
	var url = "/DemandVerification/AddDemandDetails";
	$.get(url)
		.done(function (response) {
		//	verifiedElement.style.display = 'none';
			bootbox.dialog({
				title: 'Demand Details',
				message: response,
				size: 'extra-large',
				modal: true,

			});
		});
}

function editDemandVerifyDetail(id) {
	//console.log(id);
	var url = "/DemandVerification/AddDemandDetails?VerifyId=" + id;
	$.get(url)
		.done(function (response) {
			var myBootbox = bootbox.dialog({
				title: 'Demand Details',
				message: response,
				size: 'extra-large',
				modal: true,
			});
		});
}

function saveVerificationDetails() {
	$.validator.unobtrusive.parse("form");
	var form = $('#demandVerifyDetails');
	console.log(form.valid());
	if (form.valid()) {
		//var demandVerifyDetails = [];
		var details = {};
		details.Verification_Id = document.getElementById("Verify_Id").value;
		details.Demand_No = document.getElementById("demand_No").value;
		details.Demand_Desc = document.getElementById("demand_desc").value;

		details.Demanding_Officer = document.getElementById("do").value;
		details.Project = document.getElementById("project").value;
		details.Items = document.getElementById("items").checked;
		details.Receipt = document.getElementById("receipts").checked;
		details.Receipt_Docs = document.getElementById("receipt_docs").checked;
		details.Remarks = document.getElementById("remarks").value;
		details.Verified = document.getElementById("verified").checked;

		//demandVerifyDetails.push(details);
		//console.log(JSON.stringify(details));
		$.ajax({
			type: "POST",
			url: '/DemandVerification/AddDemandDetails/',
			data: JSON.stringify(details),
			cache: false,
			contentType: 'application/json',
			success: (function (response) {
				if (response.success) {
					console.log(response.id);
					document.getElementById("Verify_Id").value = response.id;
					verifiedDiv.style.display = '';
					window.location.reload();
					alert('Updated Successfully');
				} else {
					alert('Something went wrong. Please try again later');
				}
				return false;
			}),
			error: (function (error) {
				console.log(error);
				alert('Something went wrong. Please try again later');
			})
		});
	}
	else {
		return false;
	}
}
function DeleteDemandVerifyRow(deleteElement, id) {
	console.log(deleteElement, id);
	$.ajax({
		type: "POST",
		url: '/DemandVerification/DeleteVerificationRow/?Verifyid='+id,
		success: (function (response) {
			if (response.success) {
				window.location.reload();
				alert('Updated Successfully');
			} else {
				alert('Something went wrong. Please try again later');
			}
			return false;
		}),
		error: (function (error) {
			console.log(error);
			alert('Something went wrong. Please try again later');
		})
	});
}