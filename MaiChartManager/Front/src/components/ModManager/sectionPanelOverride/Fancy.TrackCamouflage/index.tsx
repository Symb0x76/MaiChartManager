import { defineComponent } from 'vue';
import { NA } from "naive-ui";
import { useI18n } from 'vue-i18n';

export default defineComponent({
  setup() {
    const { t } = useI18n();

    return () => <div class="m-l-35">
      <NA
        // @ts-ignore
        target="_blank" href="https://on9.moe/laundry/camouflage.html"
      >
        {t('mod.trackCamouflage.viewDoc')}
      </NA>
    </div>
  },
});
