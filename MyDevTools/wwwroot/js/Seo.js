function updateMetaTags(description, keywords) {
    document.querySelector('meta[name="description"]').setAttribute("content", description);
    document.querySelector('meta[name="keywords"]').setAttribute("content", keywords);
}
