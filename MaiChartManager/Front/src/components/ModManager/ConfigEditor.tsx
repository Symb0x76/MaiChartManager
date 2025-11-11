import { computed, defineComponent, onMounted, ref, watch } from "vue";
import { NButton, NCheckbox, NFlex, NModal, NSwitch, useDialog, useMessage } from "naive-ui";
import api from "@/client/api";
import { globalCapture, modInfo, updateModInfo, updateMusicList, aquaMaiConfig as config, modUpdateInfo } from "@/store/refs";
import AquaMaiConfigurator from "./AquaMaiConfigurator";
import { compareVersions, latestVersion, shouldShowUpdate } from "./shouldShowUpdateController";
import { useStorage } from "@vueuse/core";
import _ from "lodash";
import ModInstallDropdown from "@/components/ModManager/ModInstallDropdown";
import styles from "./styles.module.sass";
import { useI18n } from 'vue-i18n';
import { debounce } from 'perfect-debounce';
import AquaMaiSignatureStatusDisplay from "./AquaMaiSignatureStatusDisplay";

export default defineComponent({
  props: {
    show: Boolean,
    badgeType: String,
  },
  setup(props, { emit }) {
    const show = computed({
      get: () => props.show,
      set: (val) => emit('update:show', val)
    })

    const configReadErr = ref('')
    const configReadErrTitle = ref('')
    const dialog = useDialog()
    const installingMelonLoader = ref(false)
    const message = useMessage();
    const { t } = useI18n();
    const isConfigCorrupted = ref(false);
    const errTitle = ref('');

    const updateAquaMaiConfig = async (forceDefault = false) => {
      try {
        configReadErr.value = ''
        configReadErrTitle.value = ''
        config.value = (await api.GetAquaMaiConfig({forceDefault})).data;
        isConfigCorrupted.value = false;
      } catch (err: any) {
        errTitle.value = t('mod.needInstallOrUpdate');
        if (err instanceof Response && !err.bodyUsed) {
            const text = await err.text();
            try {
              const json = JSON.parse(text);
              if (json.detail) {
                configReadErr.value = json.detail;
              }
              if (json.title) {
                configReadErrTitle.value = json.title;
              }
              if(configReadErrTitle.value === 'System.Reflection.TargetInvocationException' && compareVersions(modInfo.value?.aquaMaiVersion || '0.0.0', '1.6.0') < 0) {
                configReadErr.value = t('mod.versionTooLow');
              }
              if(configReadErrTitle.value.includes('ConfigCorruptedException')) {
                isConfigCorrupted.value = true;
              }
              if(configReadErr.value.includes('Could not migrate the config')) {
                errTitle.value = t('mod.configVersionHigher');
              }
              return
            } catch {
            }
            configReadErr.value = text.split('\n')[0];
        }
        if (err.error instanceof Error) {
          configReadErr.value = err.error.message.split('\n')[0];
        }
        else if (err.error) {
          configReadErr.value = err.error.toString().split('\n')[0];
        }
        configReadErr.value = err.toString().split('\n')[0];
      }
    }

    onMounted(updateAquaMaiConfig)

    const installMelonLoader = async () => {
      try {
        installingMelonLoader.value = true
        await api.InstallMelonLoader()
        await updateModInfo()
      } catch (e: any) {
        globalCapture(e, t('mod.installMelonLoaderFailed'))
      } finally {
        installingMelonLoader.value = false
      }
    }

    const saveImpl = async () => {
      if (!config.value) return;
      try {
        await api.SetAquaMaiConfig(config.value)
        await updateMusicList()
        message.success(t('music.save.saveSuccess'))
      } catch (e) {
        globalCapture(e, t('mod.saveConfigFailed'))
      }
    }
    const save = debounce(saveImpl, 2000);

    watch(() => config.value, async (val) => {
      if (configReadErr.value) return
      if (!show.value) return
      if (val) {
        console.log('配置变动')
        save()
      }
    }, { deep: true })

    const resetToDefault = async () => {
      await updateAquaMaiConfig(true);
      await saveImpl();
    }


    return () => {

      let editorPart = <></>;
      if (isConfigCorrupted.value) {
        editorPart = <NFlex vertical justify="center" align="center" class="min-h-100">
          <div class="text-8">{t('mod.configCorrupted')}</div>
          <div class="c-gray-5 text-lg">{t('mod.configCorruptedMessage')}</div>
          <div>
            <NButton secondary onClick={resetToDefault}>
              {t('mod.resetToDefault')}
            </NButton>
          </div>
        </NFlex>
      }
      else if (configReadErr.value) {
        editorPart = <NFlex vertical justify="center" align="center" class="min-h-100">
          <div class="text-8">{errTitle.value}</div>
          <div class="c-gray-5 text-lg">{configReadErr.value}</div>
          <div class="c-gray-4 text-sm">{configReadErrTitle.value}</div>
        </NFlex>
      }
      else {
        editorPart = <AquaMaiConfigurator config={config.value!} useNewSort={true}/>
      }

      return <NModal
        preset="card"
        class={["w-[min(99dvw,100em)]", styles.modal]}
        title={t('mod.title')}
        v-model:show={show.value}
      >
        {!!modInfo.value && <NFlex vertical>
          <NFlex align="center">
            <span class="max-[1060px]:hidden">MelonLoader:</span>
            {modInfo.value.melonLoaderInstalled ? <span class="c-green-6 max-[1060px]:hidden">{t('mod.installed')}</span> : <span class="c-red-6">{t('mod.notInstalled')}</span>}
            {!modInfo.value.melonLoaderInstalled && <NButton secondary loading={installingMelonLoader.value} onClick={installMelonLoader}>{t('mod.install')}</NButton>}
            <div class={["w-8", "max-[1060px]:hidden"]}/>
            <span class="max-[1060px]:hidden">AquaMai:</span>
            {modInfo.value.aquaMaiInstalled ?
              !shouldShowUpdate.value ? <span class="c-green-6 max-[1060px]:hidden">{t('mod.installed')}</span> : <span class="c-orange">{t('mod.updateAvailable')}</span> :
              <span class="c-red-6">{t('mod.notInstalled')}</span>}
            <ModInstallDropdown updateAquaMaiConfig={updateAquaMaiConfig}/>
            <span class="max-[1060px]:hidden">{t('mod.installedVersion')}:</span>
            <span class="max-[450px]:hidden">{modInfo.value.aquaMaiVersion !=='N/A' && 'v'}{modInfo.value.aquaMaiVersion}</span>
            <AquaMaiSignatureStatusDisplay/>
            <span class="max-[1060px]:hidden">{t('mod.availableVersion')}:</span>
            <span class={[shouldShowUpdate.value && "c-orange", "max-[1060px]:hidden"]}>{latestVersion.value.version}</span>
            <NButton secondary onClick={() => api.KillGameProcess()}>
              {t('mod.killGameProcess')}
            </NButton>
          </NFlex>
          {editorPart}
        </NFlex>}
      </NModal>
    };
  }
})
