import Vue from 'vue'
import axios from 'axios'
import router from './router/index'
import store from './store'
import { sync } from 'vuex-router-sync'
import App from 'components/app-root'
import { FontAwesomeIcon } from './icons'
import { L } from 'vue2-leaflet'
import 'leaflet/dist/leaflet.css'


// Registration of global components
Vue.component('icon', FontAwesomeIcon)

Vue.prototype.$http = axios

sync(store, router)
delete L.Icon.Default.prototype._getIconUrl;

L.Icon.Default.mergeOptions({
  iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
  iconUrl: require('leaflet/dist/images/marker-icon.png'),
  shadowUrl: require('leaflet/dist/images/marker-shadow.png')
});
const app = new Vue({
  store,
  router,
  App
})

export {
  app,
  router,
  store
}
