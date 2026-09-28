import {picture} from './pictures.mjs';

// Science tools and the reader use the same pictures, labels and button semantics.
export function labelButton(button,label,icon){
 button.innerHTML=picture(icon);
 const text=document.createElement('span');text.textContent=label;button.append(text);
 button.setAttribute('aria-label',label);
}
export function picButton(label,icon,click,className=''){
 const button=document.createElement('button');button.type='button';button.className=className;
 labelButton(button,label,icon);button.onclick=click;return button;
}
