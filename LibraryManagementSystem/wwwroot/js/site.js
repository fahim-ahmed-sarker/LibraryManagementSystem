// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.querySelectorAll('[data-shelf-target]').forEach((button) => {
	button.addEventListener('click', () => {
		const shelf = document.getElementById(button.dataset.shelfTarget);
		const direction = Number(button.dataset.shelfDirection);

		if (shelf) {
			shelf.scrollBy({
				left: direction * shelf.clientWidth * 0.8,
				behavior: 'smooth'
			});
		}
	});
});
