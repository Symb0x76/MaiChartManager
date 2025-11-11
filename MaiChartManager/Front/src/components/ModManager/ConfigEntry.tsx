import { IEntryState, Entry } from "@/client/apiGen";
import { NFormItem, NFlex, NSelect, NSwitch, NInput, NInputNumber } from "naive-ui";
import { computed, defineComponent, PropType } from "vue";
import ProblemsDisplay from "../ProblemsDisplay";
import { KeyCodeName } from "./types/KeyCodeName";
import { getNameForPath } from "./utils";
import comments from "./modComments.yaml";
import { KeyCodeID } from "./types/KeyCodeID";
import { useI18n } from 'vue-i18n';
import { locale } from "@/locales";

export default defineComponent({
  props: {
    entry: { type: Object as PropType<Entry>, required: true },
    entryState: { type: Object as PropType<IEntryState>, required: true },
  },
  setup(props, { emit }) {
    const { t, te } = useI18n();

    const optionsIoKeyMap = [
      { label: t('mod.adxHid.disabled'), value: 'None' },
      { label: t('mod.adxHid.select1P'), value: 'Select1P' },
      { label: t('mod.adxHid.select2P'), value: 'Select2P' },
      { label: t('mod.adxHid.service'), value: 'Service' },
      { label: t('mod.adxHid.test'), value: 'Test' },
    ];

    const optionsSoundChannel = ['None', 'P1SpeakerLeft', 'P1SpeakerRight', 'P1HeadphoneLeft', 'P1HeadphoneRight', 'P2SpeakerLeft', 'P2SpeakerRight', 'P2HeadphoneLeft', 'P2HeadphoneRight']
      .map(channel => ({ label: t('mod.soundChannel.' + channel), value: channel }));

    const comment = computed(() => {
      const localeKey = 'mod.commentOverrides.' + props.entry.path!.replace(/\./g, '_');
      if (te(localeKey)) {
        return t(localeKey);
      }
      if (locale.value.startsWith('zh')) {
        return props.entry.attribute?.comment?.commentZh;
      }
      return props.entry.attribute?.comment?.commentEn;
    })

    return () => <NFormItem label={getNameForPath(props.entry.path!, props.entry.name!, props.entry.attribute?.comment?.nameZh)} labelPlacement="left" labelWidth="9em" showFeedback={false}
      // @ts-ignore
                            title={props.entry.path!}
    >
      <NFlex vertical class="w-full ws-pre-line">
        <NFlex class="h-34px" align="center">
          {(() => {
            const choices = comments.options[props.entry.path!]
            if (choices) {
              return <NSelect v-model:value={props.entryState.value} options={choices} clearable/>
            }
            switch (props.entry.fieldType) {
              case 'System.Boolean':
                return <NSwitch v-model:value={props.entryState.value}/>;
              case 'System.String':
                return <NInput v-model:value={props.entryState.value} placeholder="" onUpdateValue={v => props.entryState.value = typeof v === 'string' ? v : ''}/>;
              case 'System.Int32':
              case 'System.Int64':
                return <NInputNumber value={props.entryState.value} onUpdateValue={v => props.entryState.value = typeof v === 'number' ? v : 0} placeholder="" precision={0} step={1}/>;
              case 'System.UInt32':
              case 'System.UInt64':
                return <NInputNumber value={props.entryState.value} onUpdateValue={v => props.entryState.value = typeof v === 'number' ? v : 0} placeholder="" precision={0} step={1} min={0}/>;
              case 'System.Byte':
                return <NInputNumber value={props.entryState.value} onUpdateValue={v => props.entryState.value = typeof v === 'number' ? v : 0} placeholder="" precision={0} step={1} min={0} max={255}/>;
              case 'System.Double':
              case 'System.Single':
                return <NInputNumber value={props.entryState.value} onUpdateValue={v => props.entryState.value = typeof v === 'number' ? v : 0} placeholder="" step={.1}/>;
              case 'AquaMai.Config.Types.KeyCodeOrName':
                return <NSelect v-model:value={props.entryState.value} options={Object.entries(KeyCodeName).map(([label, value]) => ({ label, value }))}/>;
              case 'AquaMai.Config.Types.KeyCodeID':
                return <NSelect v-model:value={props.entryState.value} options={Object.entries(KeyCodeID).map(([label, value]) => ({label, value}))}/>;
              case 'AquaMai.Config.Types.IOKeyMap':
                return <NSelect v-model:value={props.entryState.value} options={optionsIoKeyMap}/>;
              case 'AquaMai.Config.Types.AdxKeyMap':
                return <NSelect v-model:value={props.entryState.value} options={optionsIoKeyMap}/>;
              case 'AquaMai.Config.Types.SoundChannel':
                return <NSelect v-model:value={props.entryState.value} options={optionsSoundChannel}/>;
            }
            return t('mod.unsupportedType', { type: props.entry.fieldType });
          })()}
          {comments.shouldEnableOptions[props.entry.path!] && !props.entryState.value && <ProblemsDisplay problems={[t('mod.needEnableOption')]}/>}
        </NFlex>
        {comment.value}
      </NFlex>
    </NFormItem>;
  },
});
