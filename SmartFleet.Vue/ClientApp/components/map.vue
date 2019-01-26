<template>
  <l-map ref="map" :zoom="3" >
    <l-tile-layer :url="url" />
  </l-map>
</template>
<script>

  import { LMap, LTileLayer, LMarker } from 'vue2-leaflet';

  export default {
    name: 'MyAwesomeMap',
    components: {
      LMap,
      LTileLayer,
      LMarker
    },
    data() {
      return {
        url: 'http://{s}.tile.osm.org/{z}/{x}/{y}.png',
      }
    }
  }

</script>
