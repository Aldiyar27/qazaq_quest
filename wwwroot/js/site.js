document.addEventListener('DOMContentLoaded', () => {
  const toggle = document.querySelector('[data-mobile-toggle]');
  const nav = document.querySelector('[data-mobile-nav]');
  const actions = document.querySelector('.next-nav-actions');

  if (toggle && nav && actions) {
    toggle.addEventListener('click', () => {
      nav.classList.toggle('open');
      actions.classList.toggle('open');
    });
  }
});
