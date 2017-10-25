function SubsQty() {
	alert($("#txtHFFQuantity").val());
	var q = parseInt($("#txtHFFQuantity").val()) - 1;
	alert(q);
	$("#txtHFFQuantity").val(q);

	alert('launch sub');
	// Call the service
	$.ajax({
		type: 'POST',
		url: '/Ordering/Service/ShoppingCartSvc.asmx/AddHFFDonationItemToCart',
		data: { sku: 'sku', quantity: '2', distributorId: '<%= DistributorID %>', locale: 'es-MX' },
		dataType: 'xml',
		success: function (data) {

			var jData = $(data);
			var apfDue = jData.find("string").text();

			// Posible values
			// NoAPFVerification
			// BillingFailed
			// APFDue
			// String.Empty

			if (apfDue == 'BillingFailed' || apfDue == 'APFDue')
				$APFDue.retrieveSpecialMessageHTML('');

		},
		error: function showError(xhr, status, exc) {
			alert('error when calling service');
		}
	});
}

function AddQty() {
	var q = parseInt($("#txtHFFQuantity").val()) + 1;
	$("#txtHFFQuantity").val(q++);

	alert('launch add');

	// Call the service
	$.ajax(
	{
		type: 'POST',
		url: '/Ordering/Service/ShoppingCart/ShoppingCartSvc.asmx/AddHFFDonationItemToCart',
		data: { sku: 'sku', quantity: '2', distributorId: '<%= DistributorID %>', locale: 'es-MX' },
		dataType: 'xml',
		success: function (data) {

			var jData = $(data);
			var apfDue = jData.find("string").text();

			// Posible values
			// NoAPFVerification
			// BillingFailed
			// APFDue
			// String.Empty

			if (apfDue == 'BillingFailed' || apfDue == 'APFDue')
				$APFDue.retrieveSpecialMessageHTML('');
		},
		error: function showError(xhr, status, exc) {
			alert('error when calling service');
		}

	});
}

function AddOne() {
	var q = parseInt($('#' + tbQuantity).val()) + 1;
	$('#' + tbQuantity).val(q++);
}

function SubsOne() {
	var q = parseInt($('#' + tbQuantity).val());
	if (q > 1) q = q - 1;
	$('#' + tbQuantity).val(q);
}

function SetCart() {
	var location = window.location.href.replace('#', '');
	$.ajax({
		type: "POST",
		url: location + "/AddToCart",
		data: "{ quantity : " + $("#txtHFFQuantity").val() + " }",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		success: OnStep2ToSubmit()
	});
}

function OnStep1() {
	$('#' + divHFFDonation_Step1).removeClass("hide");
	$('#' + divHFFDonation_Step2).addClass("hide");

	if (e && e.preventDefault) {
	    e.preventDefault();
	}
	return false;
}

function OnStep2ToSubmit() {
	$('#' + divHFFDonation_Step1).addClass("hide");
	$('#' + divHFFDonation_Step2).removeClass("hide");
	return false;
}

function OnStep2ToEnd() {
	$('#' + divOrderComplete).removeClass("hide");
	$('#' + divEmailNotification).removeClass("hide");
	$('#' + divPayment).addClass("hide");
	$('#' + divPaymentInfo).removeClass("hide");
	$('#' + divSubmitCommand).addClass("hide");
	$('#' + divEndCommand).removeClass("hide");
	return false;
}

function OnCancel() {
    $('#' + pnlHFF).addClass("hide");

    if (e && e.preventDefault) {
        e.preventDefault();
    }
	return false;
}
