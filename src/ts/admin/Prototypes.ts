HTMLTextAreaElement.prototype.insertAt = function (text: string, index: number) {
    this.value = this.value.slice(0, index) + text + this.value.slice(index);
};