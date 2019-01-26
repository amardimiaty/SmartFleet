<template>
<div style="height:100%">
  <v-jstree :data="items" show-checkbox multiple allow-batch whole-row @item-click="itemClick"></v-jstree>

</div>
</template>
<script>import VJstree from 'vue-jstree'

export default {
    components :{
      'v-jstree': VJstree
    },
    data() {
      return {
        items: [  ]
      }
      },
      methods: {
        itemClick (node) {
          console.log(node.model.text + ' clicked !')
        }
      }

}
</script>
