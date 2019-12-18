var editor = null;

window.onload = function () {
	var element = this.document.body;
	var data = element.innerText;
	element.innerText = "";

	var mode = this.document.getElementById("mode").getAttribute("src").split('/')[3];

	editor = CodeMirror(element, {
		value: data,
		mode: mode,
		autocorrect: true,
		theme: "idea",
		indentUnit: 4,
		indentWithTabs: true,
		lineNumbers: true
	});
};

window.onkeydown = function (event) {
	if (!event.ctrlKey && !event.meta || event.key != 's') return true;
	if (save()) alert("Save successful.");
	event.preventDefault();
	return true;
};

function save() {
	var data = editor.getValue();

	try {
		JSON.parse(data);
	} catch (e) {
		alert("Cannot upload; JSON parsing failed.\nError: " + e.message);
		return;
	}

	var r = new XMLHttpRequest();
	r.open("PUT", document.documentURI, false);
	r.setRequestHeader("Content-Type", "application/json");
	r.send(data);

	return true;
}